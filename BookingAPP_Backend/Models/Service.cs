namespace BookingAPP_Backend.Models;

public class Service
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // фіксована вартість послуги за одне бронювання
    public decimal Cost { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<RoomService> RoomServices { get; set; } = new List<RoomService>();
    public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
}
