using System.Text.Json.Serialization;

namespace HotelBooking.Api.Models.Domain.Policies;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(NonRefundablePolicy), "non_refundable")]
[JsonDerivedType(typeof(FreeCancellationPolicy), "free_cancellation")]
public abstract record CancellationPolicy(string Description);

public record NonRefundablePolicy() 
    : CancellationPolicy("Non-refundable. Total price will be charged upon booking cancellation.");

public record FreeCancellationPolicy(DateTimeOffset CancelBefore) 
    : CancellationPolicy($"Free cancellation available until {CancelBefore:yyyy-MM-dd HH:mm}");