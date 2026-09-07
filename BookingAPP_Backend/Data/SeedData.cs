using BookingAPP_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPP_Backend.Data;

// наповнює базу початковими даними, описаними в технічному завданні
public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.Migrate();

        if (context.Services.Any() || context.ConferenceRooms.Any())
        {
            return; 
        }

        var projector = new Service { Id = Guid.NewGuid(), Name = "Проєктор", Cost = 500m };
        var wifi = new Service { Id = Guid.NewGuid(), Name = "Wi-Fi", Cost = 300m };
        var sound = new Service { Id = Guid.NewGuid(), Name = "Звук", Cost = 700m };

        context.Services.AddRange(projector, wifi, sound);

        var roomA = new ConferenceRoom { Id = Guid.NewGuid(), Name = "Зал А", Capacity = 50, BaseHourlyRate = 2000m };
        var roomB = new ConferenceRoom { Id = Guid.NewGuid(), Name = "Зал B", Capacity = 100, BaseHourlyRate = 3500m };
        var roomC = new ConferenceRoom { Id = Guid.NewGuid(), Name = "Зал C", Capacity = 30, BaseHourlyRate = 1500m };

        context.ConferenceRooms.AddRange(roomA, roomB, roomC);
        
        foreach (var room in new[] { roomA, roomB, roomC })
        {
            foreach (var service in new[] { projector, wifi, sound })
            {
                context.RoomServices.Add(new RoomService
                {
                    ConferenceRoomId = room.Id,
                    ServiceId = service.Id
                });
            }
        }

        context.SaveChanges();
    }
}
