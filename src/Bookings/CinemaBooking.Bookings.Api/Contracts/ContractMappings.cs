using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Api.Contracts;

public static class ContractMappings
{
    public static SeatRequest ToSeatRequest(this BookingItemRequest item) => new(item.ShowId, item.SeatIds, item.Quantity);

    public static BookingResponse ToResponse(this Booking booking) => new(
        booking.BookingId,
        Utc(booking.CreatedAt),
        booking.CancelledAt is null ? null : Utc(booking.CancelledAt.Value),
        [.. booking.BookedSeats.Select(seat => new BookedSeatResponse(seat.ShowId, seat.SeatId))]);

    public static ShowAvailabilityResponse ToResponse(this ShowAvailability availability) => new(
        availability.ShowId,
        availability.FreeSeats,
        [.. availability.Seats.Select(seat =>
            new SeatAvailabilityResponse(seat.Seat.SeatId, seat.Seat.RowLabel, seat.Seat.SeatNumber, seat.IsFree))]);

    // The database stores UTC times without an offset.
    private static DateTimeOffset Utc(DateTime value) => new(value, TimeSpan.Zero);
}
