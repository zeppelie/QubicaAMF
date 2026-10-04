namespace CinemaBooking.Catalog.Api.Contracts;

public sealed record MovieResponse(int MovieId, string Title, int DurationMinutes);