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
        if (await HallIsBusyAsync(hallId, startsAt, endsAt, cancellationToken))
            return ScheduleShowResult.Failed(ScheduleShowError.HallBusy);

        var show = new Show { MovieId = movieId, HallId = hallId, StartsAt = startsAt };
        return await repository.TryAddShowAsync(show, cancellationToken)
            ? ScheduleShowResult.Scheduled(show)
            : ScheduleShowResult.Failed(ScheduleShowError.HallBusy);
    }

    private async Task<bool> HallIsBusyAsync(int hallId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        // A show that began the day before can still be running: no movie lasts longer than that.
        var nearbyShows = await repository.ListShowsAsync(from.AddDays(-1), to, hallId, cancellationToken);
        return nearbyShows.Any(show => show.Overlaps(from, to));
    }
}