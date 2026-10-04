using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

public sealed record BookingResult(Booking? Booking, BookingError? Error, int? ShowId)
{
    public static BookingResult Booked(Booking booking) => new(booking, null, null);

    public static BookingResult Failed(BookingError error, int? showId = null) => new(null, error, showId);
}