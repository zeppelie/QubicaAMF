namespace CinemaBooking.Bookings.Core;

/// <summary>What Bookings needs to know about shows, which belong to the Catalog service.</summary>
public interface IShowCatalog
{
    /// <summary>Returns the show with the seats of its hall, or null when it does not exist.</summary>
    Task<ShowInfo?> FindShowAsync(int showId, CancellationToken cancellationToken);
}