namespace CinemaBooking.Bookings.Core;

public interface IAvailabilityService
{
    /// <summary>Returns every seat of the show marked free or taken, or null when the show does not exist.</summary>
    Task<ShowAvailability?> GetAsync(int showId, CancellationToken cancellationToken);
}