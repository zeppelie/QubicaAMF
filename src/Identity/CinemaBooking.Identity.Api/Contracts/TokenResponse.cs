namespace CinemaBooking.Identity.Api.Contracts;

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);