namespace HotelBooking.Api.Models.DTO.Requests;

public record AvailabilitySearchRequest(
    Guid HotelId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int NumberOfRooms,
    int AdultsCount,
    IEnumerable<int>? ChildrenAges
);