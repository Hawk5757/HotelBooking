namespace HotelBooking.Api.Models.DTO.Responses;

public record AvailabilitySearchResponse(
    Guid HotelId,
    IEnumerable<RoomResponse> AvailableRooms
);