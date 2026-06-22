using HotelBooking.Api.Models.DTO.Requests;
using HotelBooking.Api.Models.DTO.Responses;

namespace HotelBooking.Api.Services;

public interface IBookingSearchService
{
    Task<AvailabilitySearchResponse> SearchAvailabilityAsync(
        AvailabilitySearchRequest request, 
        CancellationToken cancellationToken);
}