using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using HotelBooking.Api.Infrastructure;
using HotelBooking.Api.Models.Domain;
using HotelBooking.Api.Models.DTO.Requests;
using HotelBooking.Api.Services;

namespace HotelBooking.Tests;

public class BookingSearchServiceTests : IDisposable
{
    private readonly HotelDbContext _dbContext;
    private readonly BookingSearchService _searchService;
    private readonly Guid _testHotelId;

    public BookingSearchServiceTests()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(databaseName: $"HotelBookingTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new HotelDbContext(options);
        _searchService = new BookingSearchService(_dbContext);
        _testHotelId = Guid.NewGuid();

        SeedMockHotel();
    }

    private void SeedMockHotel()
    {
        var hotel = new Hotel(_testHotelId, "Test Luxury Hotel", "Kyiv");
        _dbContext.Hotels.Add(hotel);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldThrowArgumentException_WhenCheckInIsInPast()
    {
        // Arrange
        var request = new AvailabilitySearchRequest(
            _testHotelId,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(5),
            1, 2, null
        );

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Booking cannot start in the past.");
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldThrowArgumentException_WhenCheckOutIsBeforeOrEqualToCheckIn()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.AddDays(2);
        var request = new AvailabilitySearchRequest(
            _testHotelId,
            checkIn,
            checkIn,
            1, 2, null
        );

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Check-out date must be after check-in date.");
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldThrowArgumentException_WhenBookingIsMoreThanOneYearInAdvance()
    {
        // Arrange
        var request = new AvailabilitySearchRequest(
            _testHotelId,
            DateTime.UtcNow.AddYears(1).AddDays(2),
            DateTime.UtcNow.AddYears(1).AddDays(5),
            1, 2, null
        );

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Booking cannot be created more than one year in advance.");
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldThrowArgumentException_WhenStayDurationIsOverOneMonth()
    {
        // Arrange
        var checkIn = DateTime.UtcNow.AddDays(1);
        var request = new AvailabilitySearchRequest(
            _testHotelId,
            checkIn,
            checkIn.AddDays(32),
            1, 2, null
        );

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Maximum stay duration is one month.");
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldThrowKeyNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        var nonExistentHotelId = Guid.NewGuid();
        var request = new AvailabilitySearchRequest(
            nonExistentHotelId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(5),
            1, 2, null
        );

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Hotel with ID {nonExistentHotelId} not found.");
    }

    [Fact]
    public async Task SearchAvailabilityAsync_ShouldOperationCanceledException_WhenTokenIsCancelled()
    {
        // Arrange
        var request = new AvailabilitySearchRequest(
            _testHotelId,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(5),
            1, 2, null
        );
        
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _searchService.SearchAvailabilityAsync(request, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}