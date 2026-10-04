using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Infrastructure.Catalog;
using CinemaBooking.Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaBooking.Bookings.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the database access and the Catalog client of the Bookings service.</summary>
    public static IServiceCollection AddBookingsInfrastructure(this IServiceCollection services, string connectionString, Uri catalogUrl)
    {
        services.AddDbContext<BookingsDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddHttpClient<IShowCatalog, CatalogShowClient>(http =>
        {
            http.BaseAddress = catalogUrl;
            http.Timeout = TimeSpan.FromSeconds(5);
        });
        return services;
    }
}
