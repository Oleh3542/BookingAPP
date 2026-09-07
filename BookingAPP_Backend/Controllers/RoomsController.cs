using BookingAPP_Backend.DTOs;
using BookingAPP_Backend.Middleware;
using BookingAPP_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPP_Backend.Controllers;

// управління конференц-залами: додавання, редагування, видалення та пошук доступних залів
[ApiController]
[Route("api/v1/rooms")]
[Produces("application/json")]
public class RoomsController : ControllerBase
{
    private readonly IRoomManagementService _roomService;

    public RoomsController(IRoomManagementService roomService)
    {
        _roomService = roomService;
    }

    // отриманя всіх списків залів
    [HttpGet]
    [ProducesResponseType(typeof(List<RoomResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoomResponseDto>>> GetAll(CancellationToken ct)
        => Ok(await _roomService.GetAllAsync(ct));

    // отриманя інформації про конкретний зал за ID
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await _roomService.GetByIdAsync(id, ct));

  
    // пошук доступних залів за датою
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<RoomResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoomResponseDto>>> Search([FromQuery] RoomSearchRequest request, CancellationToken ct)
        => Ok(await _roomService.SearchAvailableAsync(request, ct));

    // додати новий конференц-зал
    [HttpPost]
    [ApiKeyAuth]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RoomResponseDto>> Create([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var created = await _roomService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// оновити інформацію про зал
    [HttpPut("{id:guid}")]
    [ApiKeyAuth]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> Update(Guid id, [FromBody] UpdateRoomRequest request, CancellationToken ct)
        => Ok(await _roomService.UpdateAsync(id, request, ct));

 
    /// видалити конференц-зал

    [HttpDelete("{id:guid}")]
    [ApiKeyAuth]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _roomService.DeleteAsync(id, ct);
        return NoContent();
    }
    [HttpGet("availability")]
    [ProducesResponseType(typeof(List<RoomAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoomAvailabilityDto>>> Availability([FromQuery] RoomSearchRequest request, CancellationToken ct)
        => Ok(await _roomService.GetAllWithAvailabilityAsync(request, ct));
}
