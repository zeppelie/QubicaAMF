namespace CinemaBooking.Security;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);