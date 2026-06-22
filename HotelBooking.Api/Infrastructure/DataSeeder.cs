using Bogus;
using HotelBooking.Api.Models.Domain;

namespace HotelBooking.Api.Infrastructure;

public static class DataSeeder
{
    public static void Seed(HotelDbContext context)
    {
        if (context.Hotels.Any()) return;

        Randomizer.Seed = new Random(42);

        var hotelFaker = new Faker<Hotel>()
            .CustomInstantiator(f => new Hotel(
                Guid.NewGuid(),
                f.Company.CompanyName() + " Resort & Spa",
                f.Address.City()
            ));

        var hotels = hotelFaker.Generate(3);

        var allRooms = new List<Room>();
        var allRatePlans = new List<RatePlan>();

        var roomTypes = new[] { "Standard Room", "Deluxe Suite", "Family Apartment" };

        foreach (var hotel in hotels)
        {
            for (int i = 0; i < 5; i++)
            {
                var roomId = Guid.NewGuid();
                var currentType = roomTypes[i % roomTypes.Length];

                var room = new Room(roomId, hotel.Id, $"{currentType} #{i + 1}", currentType);
                allRooms.Add(room);

                var ratePlan1 = new RatePlan(Guid.NewGuid(), roomId, "Standard Rate (Non-Refundable)")
                {
                    PricePerNight = currentType == "Standard Room" ? 80.00m : 150.00m,
                    MealPlan = "Breakfast Included",
                    IsRefundable = false,
                    FreeCancellationDaysBefore = 0
                };

                var ratePlan2 = new RatePlan(Guid.NewGuid(), roomId, "Flexible Rate (Free Cancellation)")
                {
                    PricePerNight = (currentType == "Standard Room" ? 80.00m : 150.00m) * 1.25m,
                    MealPlan = "All Inclusive",
                    IsRefundable = true,
                    FreeCancellationDaysBefore = 3
                };

                allRatePlans.Add(ratePlan1);
                allRatePlans.Add(ratePlan2);
            }
        }

        context.Hotels.AddRange(hotels);
        context.Rooms.AddRange(allRooms);
        context.RatePlans.AddRange(allRatePlans);
        context.SaveChanges();
    }
}