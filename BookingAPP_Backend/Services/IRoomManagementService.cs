using BookingAPP_Backend.DTOs;

namespace BookingAPP_Backend.Services;

public interface IRoomManagementService
{
    Task<List<RoomResponseDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoomResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RoomResponseDto> CreateAsync(CreateRoomRequest request, CancellationToken ct = default);
    Task<RoomResponseDto> UpdateAsync(Guid id, UpdateRoomRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<RoomResponseDto>> SearchAvailableAsync(RoomSearchRequest request, CancellationToken ct = default);
    Task<List<RoomAvailabilityDto>> GetAllWithAvailabilityAsync(RoomSearchRequest request, CancellationToken ct = default);
}
