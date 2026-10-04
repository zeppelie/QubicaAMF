using CinemaBooking.Catalog.Core;
using CinemaBooking.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaBooking.Catalog.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the database access of the Catalog service.</summary>
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        return services;
    }
}
