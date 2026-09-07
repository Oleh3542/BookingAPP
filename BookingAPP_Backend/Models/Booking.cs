namespace BookingAPP_Backend.Models;

public enum BookingStatus
{
    Confirmed = 0,
    Cancelled = 1
}

// бронювання конференц-залу на конкретний проміжок часу

public class Booking
{
    public Guid Id { get; set; }

    public Guid ConferenceRoomId { get; set; }
    public ConferenceRoom ConferenceRoom { get; set; } = null!;

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // вартість оренди самого залу розрахована з урахуванням тарифних поясів
    public decimal RoomCost { get; set; }
  
    public decimal ServicesCost { get; set; }

    public decimal TotalCost { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}

public class BookingService
{
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }
}
