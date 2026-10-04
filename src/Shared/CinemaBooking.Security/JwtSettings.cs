using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Security;

public sealed class JwtSettings
{
    public const string Section = "Jwt";

    [Required]
    public string Issuer { get; init; } = "";

    [Required]
    public string Audience { get; init; } = "";

    [Required, MinLength(32)]
    public string Key { get; init; } = "";

    [Range(1, 1440)]
    public int LifetimeMinutes { get; init; } = 60;

    public SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(Key));
}
