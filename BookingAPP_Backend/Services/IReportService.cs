using BookingAPP_Backend.DTOs;

namespace BookingAPP_Backend.Services;

public interface IReportService
{
    Task<RevenueReportDto> GetRevenueReportAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<PopularServiceDto>> GetPopularServicesAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<UtilizationReportDto> GetRoomUtilizationAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<List<BookingsByDayDto>> GetBookingsByDayAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
}
