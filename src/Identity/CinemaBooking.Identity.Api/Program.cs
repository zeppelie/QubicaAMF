using CinemaBooking.Identity.Api.Accounts;
using CinemaBooking.Identity.Api.Entities;
using CinemaBooking.Identity.Api.Persistence;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Identity")
    ?? throw new InvalidOperationException("Connection string 'Identity' is missing.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddJwtSecurity(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
builder.Services.AddSingleton<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

var app = builder.Build();

app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("Cinema Identity API"));
app.MapHealthChecks("/health");

app.Run();
