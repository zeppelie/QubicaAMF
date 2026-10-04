using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaBooking.Bookings.Tests;

public sealed class BookingsApiFactory : WebApplicationFactory<Program>
{
    private readonly int _firstShowId = Random.Shared.Next(1_000_000, int.MaxValue - 100);
    private int _showsHandedOut;

    public int Alice { get; } = Random.Shared.Next(1_000_000, int.MaxValue - 1);

    public int Bob => Alice + 1;

    public int NextShowId() => _firstShowId + Interlocked.Increment(ref _showsHandedOut);

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            var seats = Halls.WithRows("AB", seatsPerRow: 3);
            var shows = Enumerable.Range(_firstShowId + 1, 50)
                .Select(showId => new ShowInfo(showId, DateTime.UtcNow.AddDays(1), seats))
                .ToArray();
            services.AddScoped<IShowCatalog>(_ => new FakeShowCatalog(shows));
        });

    public override async ValueTask DisposeAsync()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
            await db.BookedSeats.Where(seat => seat.Booking.UserId == Alice || seat.Booking.UserId == Bob).ExecuteDeleteAsync();
            await db.Bookings.Where(booking => booking.UserId == Alice || booking.UserId == Bob).ExecuteDeleteAsync();
        }

        await base.DisposeAsync();
    }
}
