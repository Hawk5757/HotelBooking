using HotelBooking.Api.Models.Domain.Policies;

namespace HotelBooking.Api.Models.Domain;

public record RatePlan(Guid Id, Guid RoomId, string Name)
{
    public decimal PricePerNight { get; init; }
    
    public string MealPlan { get; init; } = "Room Only";
    
    public bool IsRefundable { get; init; }
    
    public int FreeCancellationDaysBefore { get; init; }

    public decimal CalculateTotalPrice(int duration) => PricePerNight * duration;

    public CancellationPolicy GetCancellationPolicy(DateTime checkInDate)
    {
        return IsRefundable
            ? new FreeCancellationPolicy(new DateTimeOffset(checkInDate).AddDays(-FreeCancellationDaysBefore))
            : new NonRefundablePolicy();
    }
}