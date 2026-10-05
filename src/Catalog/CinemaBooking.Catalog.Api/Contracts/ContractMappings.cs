using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Api.Contracts;

public static class ContractMappings
{
    public static HallResponse ToResponse(this Hall hall) => new(hall.HallId, hall.Name);

    public static SeatResponse ToResponse(this Seat seat) => new(seat.SeatId, seat.RowLabel, seat.SeatNumber);

    public static MovieResponse ToResponse(this Movie movie) => new(movie.MovieId, movie.Title, movie.DurationMinutes);

    public static ShowResponse ToResponse(this Show show) => new(
        show.ShowId,
        show.MovieId,
        show.Movie.Title,
        show.HallId,
        show.Hall.Name,
        Utc(show.StartsAt),
        Utc(show.EndsAt));

    private static DateTimeOffset Utc(DateTime value) => new(value, TimeSpan.Zero);
}