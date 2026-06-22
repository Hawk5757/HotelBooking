using HotelBooking.Api.Extensions;
using HotelBooking.Api.Infrastructure;
using HotelBooking.Api.Models.DTO.Requests;
using HotelBooking.Api.Models.DTO.Responses;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public class BookingSearchService(HotelDbContext context) : IBookingSearchService
{
    public async Task<AvailabilitySearchResponse> SearchAvailabilityAsync(
        AvailabilitySearchRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequestOrThrow(request);

        var hotelExists = await context.Hotels
            .AnyAsync(h => h.Id == request.HotelId, cancellationToken);

        if (!hotelExists)
        {
            throw new KeyNotFoundException($"Hotel with ID {request.HotelId} not found.");
        }

        var duration = (request.CheckOutDate.Date - request.CheckInDate.Date).Days;

        var rooms = await context.Rooms
            .Include(r => r.RatePlans)
            .Where(r => r.HotelId == request.HotelId)
            .ToListAsync(cancellationToken);

        var availableRoomsResponses = rooms
            .Select(room => room.ToResponse(duration, request.CheckInDate))
            .ToList();

        return new AvailabilitySearchResponse(request.HotelId, availableRoomsResponses);
    }

    private static void ValidateRequestOrThrow(AvailabilitySearchRequest request)
    {
        var today = DateTime.UtcNow.Date;

        if (request.CheckInDate.Date < today)
        {
            throw new ArgumentException("Booking cannot start in the past.");
        }

        if (request.CheckOutDate.Date <= request.CheckInDate.Date)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        if (request.CheckInDate.Date > today.AddYears(1))
        {
            throw new ArgumentException("Booking cannot be created more than one year in advance.");
        }

        var duration = (request.CheckOutDate.Date - request.CheckInDate.Date).Days;
        if (duration > 31)
        {
            throw new ArgumentException("Maximum stay duration is one month.");
        }
    }
}