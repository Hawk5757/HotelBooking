using HotelBooking.Api.Models.Domain;
using HotelBooking.Api.Models.DTO.Responses;

namespace HotelBooking.Api.Extensions;

public static class MappingExtensions
{
    public static RoomResponse ToResponse(this Room room, int duration, DateTime checkInDate)
    {
        return new RoomResponse(
            room.Id,
            room.Name,
            room.Type,
            room.RatePlans.Select(plan => new RatePlanResponse(
                plan.Id,
                plan.Name,
                plan.CalculateTotalPrice(duration),
                plan.MealPlan,
                plan.GetCancellationPolicy(checkInDate)
            ))
        );
    }
}