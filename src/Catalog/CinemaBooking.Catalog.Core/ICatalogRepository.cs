using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Core;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Hall>> ListHallsAsync(CancellationToken cancellationToken);

    Task<bool> HallExistsAsync(int hallId, CancellationToken cancellationToken);

    /// <summary>Ordered by row and number.</summary>
    Task<IReadOnlyList<Seat>> ListSeatsAsync(int hallId, CancellationToken cancellationToken);

    /// <summary>Ordered by title.</summary>
    Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken);

    Task<Movie?> FindMovieAsync(int movieId, CancellationToken cancellationToken);

    Task AddMovieAsync(Movie movie, CancellationToken cancellationToken);

    /// <summary>Shows starting in the period, with movie and hall loaded; pass a hall id to look at one hall only.</summary>
    Task<IReadOnlyList<Show>> ListShowsAsync(DateTime from, DateTime to, int? hallId, CancellationToken cancellationToken);

    /// <summary>Returns the show with movie and hall loaded.</summary>
    Task<Show?> FindShowAsync(int showId, CancellationToken cancellationToken);

    /// <summary>Returns false when the hall already has a show starting at that very time.</summary>
    Task<bool> TryAddShowAsync(Show show, CancellationToken cancellationToken);
}