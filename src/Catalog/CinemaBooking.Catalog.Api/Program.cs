using CinemaBooking.Catalog.Core;
using CinemaBooking.Catalog.Infrastructure;
using CinemaBooking.Security;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Connection string 'Catalog' is missing.");

builder.Services.AddSerilog(logger => logger.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options => options.AddBearerToken());
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IShowScheduler, ShowScheduler>();
builder.Services.AddCatalogInfrastructure(connectionString);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Cinema Catalog API"));
app.MapHealthChecks("/health");

app.Run();