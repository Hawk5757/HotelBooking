namespace HotelBooking.Api.Models.DTO.Responses;

public record RoomResponse(
    Guid RoomId,
    string Name,
    string Type,
    IEnumerable<RatePlanResponse> RatePlans
);