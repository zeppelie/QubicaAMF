namespace CinemaBooking.Catalog.Core;

/// <summary>Puts a movie on the programme of a hall.</summary>
public interface IShowScheduler
{
    /// <summary>Schedules the movie in the hall at the given time, unless the hall is already taken.</summary>
    Task<ScheduleShowResult> ScheduleAsync(int movieId, int hallId, DateTime startsAt, CancellationToken cancellationToken);
}
