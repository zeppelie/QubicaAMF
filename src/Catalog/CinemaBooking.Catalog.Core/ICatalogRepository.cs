using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Core;

/// <summary>Reads and stores halls, seats, movies and shows.</summary>
public interface ICatalogRepository
{
    /// <summary>Returns every hall of the cinema.</summary>
    Task<IReadOnlyList<Hall>> ListHallsAsync(CancellationToken cancellationToken);

    /// <summary>Tells whether a hall with the given id exists.</summary>
    Task<bool> HallExistsAsync(int hallId, CancellationToken cancellationToken);

    /// <summary>Returns the seats of a hall ordered by row and number.</summary>
    Task<IReadOnlyList<Seat>> ListSeatsAsync(int hallId, CancellationToken cancellationToken);

    /// <summary>Returns every movie ordered by title.</summary>
    Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken);

    /// <summary>Returns the movie with the given id, or null when it is missing.</summary>
    Task<Movie?> FindMovieAsync(int movieId, CancellationToken cancellationToken);

    /// <summary>Stores a new movie and fills in its id.</summary>
    Task AddMovieAsync(Movie movie, CancellationToken cancellationToken);

    /// <summary>Returns the shows starting in the given period, with movie and hall, optionally for one hall only.</summary>
    Task<IReadOnlyList<Show>> ListShowsAsync(DateTime from, DateTime to, int? hallId, CancellationToken cancellationToken);

    /// <summary>Returns the show with the given id, with movie and hall, or null when it is missing.</summary>
    Task<Show?> FindShowAsync(int showId, CancellationToken cancellationToken);

    /// <summary>Stores a new show and fills in its id.</summary>
    Task AddShowAsync(Show show, CancellationToken cancellationToken);
}
