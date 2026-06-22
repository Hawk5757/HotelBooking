using Microsoft.EntityFrameworkCore;
using HotelBooking.Api.Models.Domain;

namespace HotelBooking.Api.Infrastructure;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RatePlan> RatePlans => Set<RatePlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hotel>()
            .HasMany(h => h.Rooms)
            .WithOne()
            .HasForeignKey(r => r.HotelId);

        modelBuilder.Entity<Room>()
            .HasMany(r => r.RatePlans)
            .WithOne()
            .HasForeignKey(rp => rp.RoomId);
    }
}