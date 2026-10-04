using CinemaBooking.Catalog.Core;
using CinemaBooking.Catalog.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Catalog.Infrastructure.Persistence;

public sealed class CatalogRepository(CatalogDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<Hall>> ListHallsAsync(CancellationToken cancellationToken) =>
        await db.Halls.AsNoTracking().OrderBy(hall => hall.Name).ToListAsync(cancellationToken);

    public Task<bool> HallExistsAsync(int hallId, CancellationToken cancellationToken) =>
        db.Halls.AnyAsync(hall => hall.HallId == hallId, cancellationToken);

    public async Task<IReadOnlyList<Seat>> ListSeatsAsync(int hallId, CancellationToken cancellationToken) =>
        await db.Seats.AsNoTracking()
            .Where(seat => seat.HallId == hallId)
            .OrderBy(seat => seat.RowLabel).ThenBy(seat => seat.SeatNumber)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken) =>
        await db.Movies.AsNoTracking().OrderBy(movie => movie.Title).ToListAsync(cancellationToken);

    public Task<Movie?> FindMovieAsync(int movieId, CancellationToken cancellationToken) =>
        db.Movies.AsNoTracking().FirstOrDefaultAsync(movie => movie.MovieId == movieId, cancellationToken);

    public async Task AddMovieAsync(Movie movie, CancellationToken cancellationToken)
    {
        db.Movies.Add(movie);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Show>> ListShowsAsync(DateTime from, DateTime to, int? hallId, CancellationToken cancellationToken) =>
        await ShowsWithDetails()
            .Where(show => show.StartsAt >= from && show.StartsAt < to)
            .Where(show => hallId == null || show.HallId == hallId)
            .OrderBy(show => show.StartsAt)
            .ToListAsync(cancellationToken);

    public Task<Show?> FindShowAsync(int showId, CancellationToken cancellationToken) =>
        ShowsWithDetails().FirstOrDefaultAsync(show => show.ShowId == showId, cancellationToken);

    public async Task AddShowAsync(Show show, CancellationToken cancellationToken)
    {
        db.Shows.Add(show);
        await db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Show> ShowsWithDetails() =>
        db.Shows.AsNoTracking().Include(show => show.Movie).Include(show => show.Hall);
}
