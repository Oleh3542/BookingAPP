using System.ComponentModel.DataAnnotations;

namespace BookingAPP_Backend.DTOs;

public class CreateBookingRequest
{
    [Required]
    public Guid RoomId { get; set; }

    [Required(ErrorMessage = "Ім'я клієнта є обов'язковим.")]
    [StringLength(200, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeOnly TimeFrom { get; set; }

    [Required]
    public TimeOnly TimeTo { get; set; }

    // ідентифікатори обраних додаткових послуг 
    public List<Guid>? ServiceIds { get; set; }
}

public class BookingServiceLineDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal RoomCost { get; set; }
    public decimal ServicesCost { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<BookingServiceLineDto> Services { get; set; } = new();
    // пояснення розрахунку вартості по тарифних поясах
    public List<string> PriceBreakdown { get; set; } = new();
}

public class CancelBookingRequest
{
    public string? Reason { get; set; }
}
