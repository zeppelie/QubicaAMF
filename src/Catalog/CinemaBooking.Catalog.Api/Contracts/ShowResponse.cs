namespace CinemaBooking.Catalog.Api.Contracts;

public sealed record ShowResponse(
    int ShowId,
    int MovieId,
    string MovieTitle,
    int HallId,
    string HallName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);