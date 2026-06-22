using Microsoft.EntityFrameworkCore;
using Serilog;
using HotelBooking.Api.Infrastructure;
using HotelBooking.Api.Models.DTO;
using HotelBooking.Api.Models.DTO.Requests;
using HotelBooking.Api.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Hotel Booking API host...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<HotelDbContext>(options =>
        options.UseInMemoryDatabase("HotelBookingDb"));

    builder.Services.AddScoped<IBookingSearchService, BookingSearchService>();
    builder.Services.AddSingleton<ResilientInitializer>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<HotelDbContext>();
            var initializer = services.GetRequiredService<ResilientInitializer>();
            
            Log.Information("Seeding database dynamically via Bogus and Polly protection...");
            initializer.Execute(() => DataSeeder.Seed(context));
            Log.Information("Database successfully seeded.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while seeding the database.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    // Temporary endpoint for getting hotelID
    app.MapGet("/api/test-hotels", async (HotelDbContext context) => 
    {
        var hotels = await context.Hotels.Select(h => new { h.Id, h.Name, h.Location }).ToListAsync();
        return Results.Ok(hotels);
    });
    
    app.MapGet("/api/availability", async (
        [AsParameters] AvailabilitySearchRequest request,
        IBookingSearchService searchService,
        CancellationToken cancellationToken) =>
    {
        Log.Information("Received availability search request for Hotel: {HotelId}", request.HotelId);
        
        try
        {
            var result = await searchService.SearchAvailabilityAsync(request, cancellationToken);
            return Results.Ok(result);
        }
        catch (ArgumentException ex)
        {
            Log.Warning("Validation failed: {Message}", ex.Message);
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            Log.Warning("Resource not found: {Message}", ex.Message);
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred during search.");
            return Results.StatusCode(500);
        }
    })
    .WithName("SearchAvailability");

    app.MapPost("/api/bookings", (CreateBookingRequest request) =>
    {
        Log.Information("Booking attempt received for Hotel: {HotelId}, Room: {RoomId}", request.HotelId, request.RoomId);
        
        return Results.StatusCode(StatusCodes.Status501NotImplemented);
    })
    .WithName("CreateBooking");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}