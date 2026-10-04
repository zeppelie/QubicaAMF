using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

/// <summary>Reads and stores bookings.</summary>
public interface IBookingRepository
{
    /// <summary>Returns the ids of the seats currently booked for a show.</summary>
    Task<IReadOnlySet<int>> ListTakenSeatIdsAsync(int showId, CancellationToken cancellationToken);

    /// <summary>Stores a new booking, or returns false when someone else took one of its seats in the meantime.</summary>
    Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken);

    /// <summary>Returns the booking with its seats, or null when it is missing.</summary>
    Task<Booking?> FindAsync(int bookingId, CancellationToken cancellationToken);

    /// <summary>Returns the bookings of a user, newest first.</summary>
    Task<IReadOnlyList<Booking>> ListByUserAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Saves the changes made to an existing booking.</summary>
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken);
}
