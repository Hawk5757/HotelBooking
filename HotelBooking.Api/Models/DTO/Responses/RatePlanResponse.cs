using HotelBooking.Api.Models.Domain.Policies;

namespace HotelBooking.Api.Models.DTO.Responses;

public record RatePlanResponse(
    Guid RatePlanId,
    string Name,
    decimal TotalPrice,
    string MealPlan,
    CancellationPolicy CancellationPolicy
);