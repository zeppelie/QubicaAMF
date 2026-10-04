using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

/// <summary>Books seats for one or more shows and cancels bookings.</summary>
public interface IBookingService
{
    /// <summary>Books every requested seat in a single booking, or nothing at all when one request cannot be satisfied.</summary>
    Task<BookingResult> BookAsync(int userId, IReadOnlyList<SeatRequest> requests, CancellationToken cancellationToken);

    /// <summary>Cancels a booking of the user and frees its seats; returns null when it succeeds.</summary>
    Task<CancelBookingError?> CancelAsync(int userId, int bookingId, CancellationToken cancellationToken);
}

public sealed record SeatRequest(int ShowId, IReadOnlyList<int>? SeatIds, int? Quantity);

public enum BookingError
{
    InvalidRequest,
    ShowNotFound,
    ShowAlreadyStarted,
    SeatNotFound,
    SeatTaken,
    NotEnoughSeats
}

public enum CancelBookingError
{
    BookingNotFound,
    AlreadyCancelled
}

public sealed record BookingResult(Booking? Booking, BookingError? Error, int? ShowId)
{
    public static BookingResult Booked(Booking booking) => new(booking, null, null);

    public static BookingResult Failed(BookingError error, int? showId = null) => new(null, error, showId);
}
