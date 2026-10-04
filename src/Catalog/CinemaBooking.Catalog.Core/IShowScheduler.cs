namespace CinemaBooking.Catalog.Core;

public interface IShowScheduler
{
    /// <summary>Puts the movie on the programme of the hall, unless the hall is busy at that time.</summary>
    Task<ScheduleShowResult> ScheduleAsync(int movieId, int hallId, DateTime startsAt, CancellationToken cancellationToken);
}