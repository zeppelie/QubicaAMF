using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

public interface IBookingService
{
    /// <summary>Books all the requested seats as one booking, or none of them.</summary>
    Task<BookingResult> BookAsync(int userId, IReadOnlyList<SeatRequest> requests, CancellationToken cancellationToken);

    /// <summary>Returns null when the booking is missing or belongs to someone else.</summary>
    Task<Booking?> FindAsync(int userId, int bookingId, CancellationToken cancellationToken);

    /// <summary>Newest first.</summary>
    Task<IReadOnlyList<Booking>> ListAsync(int userId, CancellationToken cancellationToken);

    /// <summary>Returns null when the booking was cancelled, otherwise the reason it could not be.</summary>
    Task<CancelBookingError?> CancelAsync(int userId, int bookingId, CancellationToken cancellationToken);
}