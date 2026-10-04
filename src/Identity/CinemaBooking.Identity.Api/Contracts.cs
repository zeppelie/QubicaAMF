using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Identity.Api;

public sealed record RegisterRequest(
    [Required, StringLength(50, MinimumLength = 3)] string UserName,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public sealed record LoginRequest([Required] string UserName, [Required] string Password);

public sealed record UserResponse(int UserId, string UserName, string Role);

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
