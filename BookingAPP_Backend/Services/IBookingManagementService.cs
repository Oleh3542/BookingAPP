using BookingAPP_Backend.DTOs;

namespace BookingAPP_Backend.Services;

public interface IBookingManagementService
{
    Task<BookingResponseDto> CreateAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<BookingResponseDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BookingResponseDto>> GetAllAsync(Guid? roomId, CancellationToken ct = default);
    Task CancelAsync(Guid id, CancelBookingRequest request, CancellationToken ct = default);
}
