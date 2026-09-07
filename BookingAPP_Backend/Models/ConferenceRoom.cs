namespace BookingAPP_Backend.Models;

// конференц-зал доступний для оренди.

public class ConferenceRoom
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    // базова вартість оренди за одну стандартну годину 
    public decimal BaseHourlyRate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // послуги доступні для замовлення разом з цим залом
    public ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}


// зв'язок багато до багатьох між залом та доступними для нього послугами


public class RoomService
{
    public Guid ConferenceRoomId { get; set; }
    public ConferenceRoom ConferenceRoom { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}

