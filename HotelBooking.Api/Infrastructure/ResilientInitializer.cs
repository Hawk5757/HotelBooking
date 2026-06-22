using Polly;
using Polly.Retry;

namespace HotelBooking.Api.Infrastructure;

public class ResilientInitializer
{
    private readonly ResiliencePipeline _pipeline;

    public ResilientInitializer()
    {
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential
            })
            .Build();
    }

    public void Execute(Action action)
    {
        _pipeline.Execute(action);
    }
}