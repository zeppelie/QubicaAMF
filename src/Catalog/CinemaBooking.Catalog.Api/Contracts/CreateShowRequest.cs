namespace CinemaBooking.Catalog.Api.Contracts;

public sealed record CreateShowRequest(int MovieId, int HallId, DateTimeOffset StartsAt);