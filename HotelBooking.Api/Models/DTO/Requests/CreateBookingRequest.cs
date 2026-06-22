namespace HotelBooking.Api.Models.DTO.Requests;

public record CreateBookingRequest(
    Guid HotelId,
    Guid RoomId,
    Guid RatePlanId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string GuestFirstName,
    string GuestLastName,
    string GuestEmail
);