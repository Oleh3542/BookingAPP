using BookingAPP_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingAPP_Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ConferenceRoom> ConferenceRooms => Set<ConferenceRoom>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<RoomService> RoomServices => Set<RoomService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
 
        modelBuilder.Entity<RoomService>(entity =>
        {
            entity.HasKey(rs => new { rs.ConferenceRoomId, rs.ServiceId });

            entity.HasOne(rs => rs.ConferenceRoom)
                  .WithMany(r => r.RoomServices)
                  .HasForeignKey(rs => rs.ConferenceRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rs => rs.Service)
                  .WithMany(s => s.RoomServices)
                  .HasForeignKey(rs => rs.ServiceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // BookingService 
        modelBuilder.Entity<BookingService>(entity =>
        {
            entity.HasKey(bs => new { bs.BookingId, bs.ServiceId });

            entity.HasOne(bs => bs.Booking)
                  .WithMany(b => b.BookingServices)
                  .HasForeignKey(bs => bs.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bs => bs.Service)
                  .WithMany(s => s.BookingServices)
                  .HasForeignKey(bs => bs.ServiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(bs => bs.PriceAtBooking).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ConferenceRoom>(entity =>
        {
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.BaseHourlyRate).HasPrecision(18, 2);
            entity.HasIndex(r => r.Name);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Cost).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(b => b.CustomerEmail).IsRequired().HasMaxLength(320);
            entity.Property(b => b.RoomCost).HasPrecision(18, 2);
            entity.Property(b => b.ServicesCost).HasPrecision(18, 2);
            entity.Property(b => b.TotalCost).HasPrecision(18, 2);

            // індекс для швидкої перевірки перетину бронювань по залу і часу
            entity.HasIndex(b => new { b.ConferenceRoomId, b.StartTime, b.EndTime });

            entity.HasOne(b => b.ConferenceRoom)
                  .WithMany(r => r.Bookings)
                  .HasForeignKey(b => b.ConferenceRoomId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
