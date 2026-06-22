namespace HotelBooking.Api.Models.Domain;

public record Room(Guid Id, Guid HotelId, string Name, string Type)
{
    public ICollection<RatePlan> RatePlans { get; init; } = new List<RatePlan>();
};