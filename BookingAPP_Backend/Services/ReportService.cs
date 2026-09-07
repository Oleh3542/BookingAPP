using BookingAPP_Backend.Data;
using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPP_Backend.Services;


// звіти та аналітика, корисні для бізнесу виручка, популярність послуг

public class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RevenueReportDto> GetRevenueReportAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (start, end) = ToRange(from, to);

        var bookings = await _db.Bookings
            .Include(b => b.ConferenceRoom)
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime >= start && b.StartTime < end)
            .ToListAsync(ct);

        var byRoom = bookings
            .GroupBy(b => new { b.ConferenceRoomId, b.ConferenceRoom.Name })
            .Select(g => new RevenueByRoomDto
            {
                RoomId = g.Key.ConferenceRoomId,
                RoomName = g.Key.Name,
                BookingsCount = g.Count(),
                TotalRevenue = Math.Round(g.Sum(b => b.TotalCost), 2),
                AverageBookingValue = Math.Round(g.Average(b => b.TotalCost), 2)
            })
            .OrderByDescending(r => r.TotalRevenue)
            .ToList();

        return new RevenueReportDto
        {
            From = from,
            To = to,
            TotalRevenue = Math.Round(bookings.Sum(b => b.TotalCost), 2),
            TotalBookings = bookings.Count,
            ByRoom = byRoom
        };
    }

    public async Task<List<PopularServiceDto>> GetPopularServicesAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (start, end) = ToRange(from, to);

        var lines = await _db.BookingServices
            .Include(bs => bs.Service)
            .Include(bs => bs.Booking)
            .Where(bs => bs.Booking.Status == BookingStatus.Confirmed &&
                         bs.Booking.StartTime >= start && bs.Booking.StartTime < end)
            .ToListAsync(ct);

        return lines
            .GroupBy(l => new { l.ServiceId, l.Service.Name })
            .Select(g => new PopularServiceDto
            {
                ServiceId = g.Key.ServiceId,
                ServiceName = g.Key.Name,
                TimesBooked = g.Count(),
                TotalRevenue = Math.Round(g.Sum(l => l.PriceAtBooking), 2)
            })
            .OrderByDescending(s => s.TimesBooked)
            .ToList();
    }

    public async Task<UtilizationReportDto> GetRoomUtilizationAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (start, end) = ToRange(from, to);
        var totalDays = Math.Max(1, (end - start).Days);
        // доступний час для бронювання 
        const double availableHoursPerDay = 17.0;

        var rooms = await _db.ConferenceRooms.ToListAsync(ct);

        var bookings = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime >= start && b.StartTime < end)
            .ToListAsync(ct);

        var result = rooms.Select(room =>
        {
            var bookedHours = bookings
                .Where(b => b.ConferenceRoomId == room.Id)
                .Sum(b => (b.EndTime - b.StartTime).TotalHours);

            var availableHours = totalDays * availableHoursPerDay;

            return new RoomUtilizationDto
            {
                RoomId = room.Id,
                RoomName = room.Name,
                BookedHours = Math.Round(bookedHours, 2),
                AvailableHours = Math.Round(availableHours, 2),
                UtilizationPercent = availableHours > 0 ? Math.Round(bookedHours / availableHours * 100, 2) : 0
            };
        })
        .OrderByDescending(r => r.UtilizationPercent)
        .ToList();

        return new UtilizationReportDto { From = from, To = to, Rooms = result };
    }

    public async Task<List<BookingsByDayDto>> GetBookingsByDayAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var (start, end) = ToRange(from, to);

        var bookings = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime >= start && b.StartTime < end)
            .ToListAsync(ct);

        return bookings
            .GroupBy(b => DateOnly.FromDateTime(b.StartTime))
            .Select(g => new BookingsByDayDto
            {
                Date = g.Key,
                BookingsCount = g.Count(),
                Revenue = Math.Round(g.Sum(b => b.TotalCost), 2)
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var totalRooms = await _db.ConferenceRooms.CountAsync(ct);
        var activeCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed, ct);
        var cancelledCount = await _db.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled, ct);
        var totalRevenue = await _db.Bookings.Where(b => b.Status == BookingStatus.Confirmed).SumAsync(b => (decimal?)b.TotalCost, ct) ?? 0m;

        var last30 = DateTime.UtcNow.AddDays(-30);
        var revenueLast30 = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed && b.StartTime >= last30)
            .SumAsync(b => (decimal?)b.TotalCost, ct) ?? 0m;

        var mostPopularRoom = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Include(b => b.ConferenceRoom)
            .GroupBy(b => b.ConferenceRoom.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync(ct);

        var mostPopularService = await _db.BookingServices
            .Include(bs => bs.Service)
            .GroupBy(bs => bs.Service.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync(ct);

        return new DashboardSummaryDto
        {
            TotalRooms = totalRooms,
            ActiveBookingsCount = activeCount,
            CancelledBookingsCount = cancelledCount,
            TotalRevenueAllTime = Math.Round(totalRevenue, 2),
            RevenueLast30Days = Math.Round(revenueLast30, 2),
            MostPopularRoomName = mostPopularRoom,
            MostPopularServiceName = mostPopularService
        };
    }

    private static (DateTime start, DateTime end) ToRange(DateOnly from, DateOnly to)
    {
        var start = from.ToDateTime(TimeOnly.MinValue);
        var end = to.ToDateTime(TimeOnly.MinValue).AddDays(1); // включно з "to"
        return (start, end);
    }
}
