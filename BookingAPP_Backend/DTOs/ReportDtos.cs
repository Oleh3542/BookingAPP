namespace BookingAPP_Backend.DTOs;

public class RevenueByRoomDto
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int BookingsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageBookingValue { get; set; }
}

public class RevenueReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public List<RevenueByRoomDto> ByRoom { get; set; } = new();
}

public class PopularServiceDto
{
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int TimesBooked { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class RoomUtilizationDto
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public double BookedHours { get; set; }
    public double AvailableHours { get; set; }
    public double UtilizationPercent { get; set; }
}

public class UtilizationReportDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public List<RoomUtilizationDto> Rooms { get; set; } = new();
}

public class BookingsByDayDto
{
    public DateOnly Date { get; set; }
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalRooms { get; set; }
    public int ActiveBookingsCount { get; set; }
    public int CancelledBookingsCount { get; set; }
    public decimal TotalRevenueAllTime { get; set; }
    public decimal RevenueLast30Days { get; set; }
    public string? MostPopularRoomName { get; set; }
    public string? MostPopularServiceName { get; set; }
}
