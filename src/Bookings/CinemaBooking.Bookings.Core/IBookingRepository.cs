using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

public interface IBookingRepository
{
    /// <summary>Seats of cancelled bookings do not count.</summary>
    Task<IReadOnlySet<int>> ListTakenSeatIdsAsync(int showId, CancellationToken cancellationToken);

    /// <summary>Returns false when someone else took one of the seats in the meantime.</summary>
    Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken);

    /// <summary>Loads the booking with its seats, ready to be changed.</summary>
    Task<Booking?> FindAsync(int bookingId, CancellationToken cancellationToken);

    /// <summary>Newest first.</summary>
    Task<IReadOnlyList<Booking>> ListByUserAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Stores the changes made to the bookings loaded with FindAsync.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}