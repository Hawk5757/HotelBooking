namespace HotelBooking.Api.Models.Domain;

public record Hotel(Guid Id, string Name, string Location)
{
    public ICollection<Room> Rooms { get; init; } = new List<Room>();
}