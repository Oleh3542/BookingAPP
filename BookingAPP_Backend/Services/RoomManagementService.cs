using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Common;
using BookingAPP_Backend.Data;
using BookingAPP_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPP_Backend.Services;

public class RoomManagementService : IRoomManagementService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RoomManagementService> _logger;

    public RoomManagementService(AppDbContext db, ILogger<RoomManagementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<RoomResponseDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rooms = await _db.ConferenceRooms
            .Include(r => r.RoomServices).ThenInclude(rs => rs.Service)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

        return rooms.Select(ToDto).ToList();
    }

    public async Task<RoomResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var room = await LoadRoomAsync(id, ct);
        return ToDto(room);
    }

    public async Task<RoomResponseDto> CreateAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        if (await _db.ConferenceRooms.AnyAsync(r => r.Name == request.Name, ct))
        {
            throw new ValidationFailedException($"Зал з назвою '{request.Name}' вже існує.");
        }

        var room = new ConferenceRoom
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            BaseHourlyRate = request.BaseHourlyRate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.ConferenceRooms.Add(room);

        await AttachServicesAsync(room, request.ServiceIds, request.NewServices, ct);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Створено новий зал {RoomId} ({RoomName})", room.Id, room.Name);

        return await GetByIdAsync(room.Id, ct);
    }

    public async Task<RoomResponseDto> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken ct = default)
    {
        var room = await LoadRoomAsync(id, ct);

        if (!string.IsNullOrWhiteSpace(request.Name)) room.Name = request.Name.Trim();
        if (request.Capacity.HasValue) room.Capacity = request.Capacity.Value;
        if (request.BaseHourlyRate.HasValue) room.BaseHourlyRate = request.BaseHourlyRate.Value;
        if (request.IsActive.HasValue) room.IsActive = request.IsActive.Value;

        if (request.ServiceIds is not null || request.NewServices is not null)
        {
            _db.RoomServices.RemoveRange(room.RoomServices);
            room.RoomServices.Clear();
            await AttachServicesAsync(room, request.ServiceIds, request.NewServices, ct);
        }

        room.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Оновлено зал {RoomId}", room.Id);

        return await GetByIdAsync(room.Id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var room = await LoadRoomAsync(id, ct);

        var hasFutureBookings = await _db.Bookings.AnyAsync(
            b => b.ConferenceRoomId == id && b.Status == BookingStatus.Confirmed && b.EndTime > DateTime.UtcNow, ct);

        if (hasFutureBookings)
        {
            // Не видаляємо фізично, а деактивуємо, щоб зберегти цілісність історії бронювань
            room.IsActive = false;
            room.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            _logger.LogInformation("Зал {RoomId} деактивовано (є активні бронювання), фізичне видалення відхилено", id);
            return;
        }

        _db.ConferenceRooms.Remove(room);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Видалено зал {RoomId}", id);
    }

    public async Task<List<RoomResponseDto>> SearchAvailableAsync(RoomSearchRequest request, CancellationToken ct = default)
    {
        if (request.TimeTo <= request.TimeFrom)
        {
            throw new ValidationFailedException("Час завершення має бути пізніше за час початку.");
        }

        var start = request.Date.ToDateTime(request.TimeFrom);
        var end = request.Date.ToDateTime(request.TimeTo);

        var query = _db.ConferenceRooms
            .Include(r => r.RoomServices).ThenInclude(rs => rs.Service)
            .Where(r => r.IsActive)
            .AsQueryable();

        if (request.MinCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= request.MinCapacity.Value);
        }

        var candidateRooms = await query.ToListAsync(ct);

        var busyRoomIds = await _db.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed &&
                        b.StartTime < end && b.EndTime > start)
            .Select(b => b.ConferenceRoomId)
            .Distinct()
            .ToListAsync(ct);

        var available = candidateRooms.Where(r => !busyRoomIds.Contains(r.Id)).OrderBy(r => r.Name);

        return available.Select(ToDto).ToList();
    }

    private async Task<ConferenceRoom> LoadRoomAsync(Guid id, CancellationToken ct)
    {
        var room = await _db.ConferenceRooms
            .Include(r => r.RoomServices).ThenInclude(rs => rs.Service)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        return room ?? throw new NotFoundException("Зал", id);
    }

    private async Task AttachServicesAsync(ConferenceRoom room, List<Guid>? serviceIds, List<CreateServiceRequest>? newServices, CancellationToken ct)
    {
        if (newServices is not null)
        {
            foreach (var ns in newServices)
            {
                var existing = await _db.Services.FirstOrDefaultAsync(s => s.Name == ns.Name, ct);
                var service = existing ?? new Service { Id = Guid.NewGuid(), Name = ns.Name.Trim(), Cost = ns.Cost };
                if (existing is null)
                {
                    _db.Services.Add(service);
                }

                room.RoomServices.Add(new RoomService { ConferenceRoomId = room.Id, ServiceId = service.Id, Service = service });
            }
        }

        if (serviceIds is not null)
        {
            foreach (var serviceId in serviceIds.Distinct())
            {
                if (room.RoomServices.Any(rs => rs.ServiceId == serviceId)) continue;

                var service = await _db.Services.FindAsync(new object?[] { serviceId }, ct)
                    ?? throw new NotFoundException("Послуга", serviceId);

                room.RoomServices.Add(new RoomService { ConferenceRoomId = room.Id, ServiceId = serviceId, Service = service });
            }
        }
    }

    private static RoomResponseDto ToDto(ConferenceRoom room) => new()
    {
        Id = room.Id,
        Name = room.Name,
        Capacity = room.Capacity,
        BaseHourlyRate = room.BaseHourlyRate,
        IsActive = room.IsActive,
        AvailableServices = room.RoomServices
            .Select(rs => new ServiceDto { Id = rs.Service.Id, Name = rs.Service.Name, Cost = rs.Service.Cost })
            .OrderBy(s => s.Name)
            .ToList()
    };
}