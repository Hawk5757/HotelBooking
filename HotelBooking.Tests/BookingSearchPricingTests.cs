using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HotelBooking.Api.Infrastructure;
using HotelBooking.Api.Models.Domain;
using HotelBooking.Api.Models.Domain.Policies;
using HotelBooking.Api.Models.DTO.Requests;
using HotelBooking.Api.Services;

namespace HotelBooking.Tests;

public class BookingSearchPricingTests : IDisposable
{
    private readonly HotelDbContext _dbContext;
    private readonly BookingSearchService _searchService;
    private readonly Guid _hotelId;

    public BookingSearchPricingTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: $"HotelBookingPricingDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new HotelDbContext(options);
        _searchService = new BookingSearchService(_dbContext);
        _hotelId = Guid.NewGuid();
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldCalculateTotalPriceAndPolymorphicPoliciesCorrectly()
    {
        // Arrange
        var hotel = new Hotel(_hotelId, "Grand Palace", "Lviv");
        var roomId = Guid.NewGuid();
        var room = new Room(roomId, _hotelId, "Standard King", "Standard");
        
        var flexiblePlan = new RatePlan(Guid.NewGuid(), roomId, "Flexible Plan")
        {
            PricePerNight = 100.00m,
            IsRefundable = true,
            FreeCancellationDaysBefore = 2,
            MealPlan = "Breakfast"
        };

        var nonRefundablePlan = new RatePlan(Guid.NewGuid(), roomId, "Promo Plan")
        {
            PricePerNight = 80.00m,
            IsRefundable = false,
            FreeCancellationDaysBefore = 0,
            MealPlan = "None"
        };

        _dbContext.Hotels.Add(hotel);
        _dbContext.Rooms.Add(room);
        _dbContext.RatePlans.AddRange(flexiblePlan, nonRefundablePlan);
        await _dbContext.SaveChangesAsync();

        var checkIn = DateTime.UtcNow.Date.AddDays(5);
        var checkOut = DateTime.UtcNow.Date.AddDays(10);
        
        var request = new AvailabilitySearchRequest(_hotelId, checkIn, checkOut, 1, 2, null);

        // Act
        var response = await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.AvailableRooms.Should().HaveCount(1);
        
        var returnedRoom = response.AvailableRooms.First();
        returnedRoom.RatePlans.Should().HaveCount(2);

        // Check Flexible
        var returnedFlexible = returnedRoom.RatePlans.First(p => p.RatePlanId == flexiblePlan.Id);
        returnedFlexible.TotalPrice.Should().Be(500.00m);
        returnedFlexible.CancellationPolicy.Should().BeOfType<FreeCancellationPolicy>();
        
        var policy = (FreeCancellationPolicy)returnedFlexible.CancellationPolicy;
        policy.CancelBefore.Should().Be(new DateTimeOffset(checkIn).AddDays(-2));

        // Check Non-Refundable
        var returnedPromo = returnedRoom.RatePlans.First(p => p.RatePlanId == nonRefundablePlan.Id);
        returnedPromo.TotalPrice.Should().Be(400.00m);
        returnedPromo.CancellationPolicy.Should().BeOfType<NonRefundablePolicy>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}