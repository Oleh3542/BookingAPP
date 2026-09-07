using BookingAPP_Backend.Common;
using BookingAPP_Backend.Data;
using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPP_Backend.Services;

public class BookingManagementService : IBookingManagementService
{
    private readonly AppDbContext _db;
    private readonly ILogger<BookingManagementService> _logger;

    public BookingManagementService(AppDbContext db, ILogger<BookingManagementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<BookingResponseDto> CreateAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        if (request.TimeTo <= request.TimeFrom)
        {
            throw new ValidationFailedException("Час завершення має бути пізніше за час початку.");
        }

        var room = await _db.ConferenceRooms
            .Include(r => r.RoomServices).ThenInclude(rs => rs.Service)
            .FirstOrDefaultAsync(r => r.Id == request.RoomId, ct)
            ?? throw new NotFoundException("Зал", request.RoomId);

        if (!room.IsActive)
        {
            throw new ValidationFailedException("Цей зал наразі недоступний для бронювання.");
        }

        var start = request.Date.ToDateTime(request.TimeFrom);
        var end = request.Date.ToDateTime(request.TimeTo);

        if (start < DateTime.UtcNow.AddMinutes(-1))
        {
            throw new ValidationFailedException("Неможливо забронювати зал у минулому.");
        }

        // перевірка перетину з існуючими підтвердженими бронюваннями цього залу
        var overlaps = await _db.Bookings.AnyAsync(b =>
            b.ConferenceRoomId == request.RoomId &&
            b.Status == BookingStatus.Confirmed &&
            b.StartTime < end && b.EndTime > start, ct);

        if (overlaps)
        {
            throw new BookingConflictException(
                "Зал вже заброньований на обраний період. Будь ласка, оберіть інший час або зал.");
        }

        // розрахунок вартості оренди залу за тарифними поясами
        var pricing = PricingCalculator.CalculateRoomCost(room.BaseHourlyRate, start, end);

        // валідація та розрахунок вартості обраних послуг
        var selectedServiceIds = request.ServiceIds?.Distinct().ToList() ?? new List<Guid>();
        var bookingServices = new List<BookingService>();
        decimal servicesCost = 0m;

        foreach (var serviceId in selectedServiceIds)
        {
            var roomService = room.RoomServices.FirstOrDefault(rs => rs.ServiceId == serviceId);
            if (roomService is null)
            {
                throw new ValidationFailedException(
                    $"Послуга з ID '{serviceId}' недоступна для залу '{room.Name}'.");
            }

            servicesCost += roomService.Service.Cost;
            bookingServices.Add(new BookingService
            {
                ServiceId = serviceId,
                Service = roomService.Service,
                PriceAtBooking = roomService.Service.Cost
            });
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            ConferenceRoomId = room.Id,
            ConferenceRoom = room,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            StartTime = start,
            EndTime = end,
            RoomCost = pricing.TotalCost,
            ServicesCost = Math.Round(servicesCost, 2),
            TotalCost = Math.Round(pricing.TotalCost + servicesCost, 2),
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            BookingServices = bookingServices
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Створено бронювання {BookingId} для залу {RoomId} на {Start:O}-{End:O}, сума {Total}",
            booking.Id, room.Id, start, end, booking.TotalCost);

        return ToDto(booking, room, pricing);
    }

    public async Task<BookingResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(id, ct);
        var pricing = PricingCalculator.CalculateRoomCost(booking.ConferenceRoom.BaseHourlyRate, booking.StartTime, booking.EndTime);
        return ToDto(booking, booking.ConferenceRoom, pricing);
    }

    public async Task<List<BookingResponseDto>> GetAllAsync(Guid? roomId, CancellationToken ct = default)
    {
        var query = _db.Bookings
            .Include(b => b.ConferenceRoom)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
            .AsQueryable();

        if (roomId.HasValue)
        {
            query = query.Where(b => b.ConferenceRoomId == roomId.Value);
        }

        var bookings = await query.OrderByDescending(b => b.StartTime).ToListAsync(ct);

        return bookings.Select(b =>
        {
            var pricing = PricingCalculator.CalculateRoomCost(b.ConferenceRoom.BaseHourlyRate, b.StartTime, b.EndTime);
            return ToDto(b, b.ConferenceRoom, pricing);
        }).ToList();
    }

    public async Task CancelAsync(Guid id, CancelBookingRequest request, CancellationToken ct = default)
    {
        var booking = await LoadBookingAsync(id, ct);

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ValidationFailedException("Це бронювання вже скасовано.");
        }

        booking.Status = BookingStatus.Cancelled;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Скасовано бронювання {BookingId}. Причина: {Reason}", id, request.Reason ?? "не вказано");
    }

    private async Task<Booking> LoadBookingAsync(Guid id, CancellationToken ct)
    {
        var booking = await _db.Bookings
            .Include(b => b.ConferenceRoom)
            .Include(b => b.BookingServices).ThenInclude(bs => bs.Service)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        return booking ?? throw new NotFoundException("Бронювання", id);
    }

    private static BookingResponseDto ToDto(Booking booking, ConferenceRoom room, PricingResult pricing)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            RoomId = room.Id,
            RoomName = room.Name,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            RoomCost = booking.RoomCost,
            ServicesCost = booking.ServicesCost,
            TotalCost = booking.TotalCost,
            Status = booking.Status.ToString(),
            Services = booking.BookingServices.Select(bs => new BookingServiceLineDto
            {
                ServiceId = bs.ServiceId,
                ServiceName = bs.Service.Name,
                Price = bs.PriceAtBooking
            }).ToList(),
            PriceBreakdown = pricing.Segments
                .Select(s => $"{s.Hour:00}:00–{(s.Hour + 1):00}:00 ({s.BandName}): {s.DurationHours:0.##} год × коеф. {s.Multiplier} = {s.SegmentCost} грн")
                .ToList()
        };
    }

}
