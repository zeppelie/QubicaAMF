using System.ComponentModel.DataAnnotations;
using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Api;

public sealed record HallResponse(int HallId, string Name);

public sealed record SeatResponse(int SeatId, string RowLabel, int SeatNumber);

public sealed record MovieResponse(int MovieId, string Title, int DurationMinutes);

public sealed record CreateMovieRequest(
    [Required, StringLength(200)] string Title,
    [Range(1, 600)] int DurationMinutes);

public sealed record ShowResponse(
    int ShowId,
    int MovieId,
    string MovieTitle,
    int HallId,
    string HallName,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);

public sealed record CreateShowRequest(int MovieId, int HallId, DateTimeOffset StartsAt);

public static class ContractMappings
{
    public static HallResponse ToResponse(this Hall hall) => new(hall.HallId, hall.Name);

    public static SeatResponse ToResponse(this Seat seat) => new(seat.SeatId, seat.RowLabel, seat.SeatNumber);

    public static MovieResponse ToResponse(this Movie movie) => new(movie.MovieId, movie.Title, movie.DurationMinutes);

    public static ShowResponse ToResponse(this Show show)
    {
        var startsAt = new DateTimeOffset(show.StartsAt, TimeSpan.Zero);
        return new ShowResponse(
            show.ShowId,
            show.MovieId,
            show.Movie.Title,
            show.HallId,
            show.Hall.Name,
            startsAt,
            startsAt.AddMinutes(show.Movie.DurationMinutes));
    }
}
