using CinemaBooking.Catalog.Core.Entities;

namespace CinemaBooking.Catalog.Core;

public sealed class ShowScheduler(ICatalogRepository repository, TimeProvider clock) : IShowScheduler
{
    public async Task<ScheduleShowResult> ScheduleAsync(int movieId, int hallId, DateTime startsAt, CancellationToken cancellationToken)
    {
        if (startsAt <= clock.GetUtcNow().UtcDateTime)
            return ScheduleShowResult.Failed(ScheduleShowError.StartsInThePast);

        var movie = await repository.FindMovieAsync(movieId, cancellationToken);
        if (movie is null)
            return ScheduleShowResult.Failed(ScheduleShowError.MovieNotFound);

        if (!await repository.HallExistsAsync(hallId, cancellationToken))
            return ScheduleShowResult.Failed(ScheduleShowError.HallNotFound);

        var endsAt = startsAt.AddMinutes(movie.DurationMinutes);
        var nearbyShows = await repository.ListShowsAsync(startsAt.AddDays(-1), endsAt, hallId, cancellationToken);
        if (nearbyShows.Any(show => Overlaps(show, startsAt, endsAt)))
            return ScheduleShowResult.Failed(ScheduleShowError.HallBusy);

        var newShow = new Show { MovieId = movieId, HallId = hallId, StartsAt = startsAt };
        await repository.AddShowAsync(newShow, cancellationToken);
        return ScheduleShowResult.Scheduled(newShow);
    }

    private static bool Overlaps(Show show, DateTime startsAt, DateTime endsAt) =>
        show.StartsAt < endsAt && startsAt < show.StartsAt.AddMinutes(show.Movie.DurationMinutes);
}
