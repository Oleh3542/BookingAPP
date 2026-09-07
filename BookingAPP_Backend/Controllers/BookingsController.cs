using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPP_Backend.Controllers;

// бронювання конференц-залів та розрахунок вартості оренди
[ApiController]
[Route("api/v1/bookings")]
[Produces("application/json")]
public class BookingsController : ControllerBase
{
    private readonly IBookingManagementService _bookingService;

    public BookingsController(IBookingManagementService bookingService)
    {
        _bookingService = bookingService;
    }

    // отримати список усіх бронювань 
    [HttpGet]
    [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BookingResponseDto>>> GetAll([FromQuery] Guid? roomId, CancellationToken ct)
        => Ok(await _bookingService.GetAllAsync(roomId, ct));

    // отримати деталі конкретного бронювання з розбивкою вартості за тарифними поясами
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _bookingService.GetByIdAsync(id, ct));


    // забронювати зал 
 
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponseDto>> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var created = await _bookingService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // скасувати бронювання
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBookingRequest request, CancellationToken ct)
    {
        await _bookingService.CancelAsync(id, request, ct);
        return NoContent();
    }
}

