using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Middleware;
using BookingAPP_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPP_Backend.Controllers;


// звіти та аналітика для бізнесу

[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
[ApiKeyAuth]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // загальна виручка та кількість бронювань за період, у розрізі по залах
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RevenueReportDto>> Revenue([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _reportService.GetRevenueReportAsync(from, to, ct));

    // найпопулярніші додаткові послуги за певний період
    [HttpGet("popular-services")]
    [ProducesResponseType(typeof(List<PopularServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PopularServiceDto>>> PopularServices([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _reportService.GetPopularServicesAsync(from, to, ct));

    // завантаженість  кожного залу за період у відсотках від доступного часу
    [HttpGet("room-utilization")]
    [ProducesResponseType(typeof(UtilizationReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UtilizationReportDto>> RoomUtilization([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _reportService.GetRoomUtilizationAsync(from, to, ct));

    // кількість бронювань та виручка по днях
    [HttpGet("bookings-by-day")]
    [ProducesResponseType(typeof(List<BookingsByDayDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookingsByDayDto>>> BookingsByDay([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await _reportService.GetBookingsByDayAsync(from, to, ct));

    // зведена панель показників для головного екрану адміністратора
    [HttpGet("dashboard-summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardSummaryDto>> DashboardSummary(CancellationToken ct)
        => Ok(await _reportService.GetDashboardSummaryAsync(ct));
}
