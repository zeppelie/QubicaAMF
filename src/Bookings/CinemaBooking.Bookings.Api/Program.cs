using CinemaBooking.Bookings.Api;
using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Infrastructure;
using CinemaBooking.Security;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Bookings")
    ?? throw new InvalidOperationException("Connection string 'Bookings' is missing.");
var catalogUrl = builder.Configuration["Catalog:BaseUrl"]
    ?? throw new InvalidOperationException("Setting 'Catalog:BaseUrl' is missing.");

builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CatalogUnavailableHandler>();
builder.Services.AddOpenApi(options => options.AddBearerToken());
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddBookingsInfrastructure(connectionString, new Uri(catalogUrl));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Cinema Bookings API"));
app.MapHealthChecks("/health");

app.Run();
