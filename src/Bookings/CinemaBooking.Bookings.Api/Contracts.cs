using System.ComponentModel.DataAnnotations;
using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Api;

public sealed record BookingItemRequest(int ShowId, IReadOnlyList<int>? SeatIds, int? Quantity);

public sealed record CreateBookingRequest([Required, MinLength(1)] IReadOnlyList<BookingItemRequest> Items);

public sealed record BookedSeatResponse(int ShowId, int SeatId);

public sealed record BookingResponse(
    int BookingId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    IReadOnlyList<BookedSeatResponse> Seats);

public sealed record SeatAvailabilityResponse(int SeatId, string RowLabel, int SeatNumber, bool IsFree);

public sealed record ShowAvailabilityResponse(int ShowId, int FreeSeats, IReadOnlyList<SeatAvailabilityResponse> Seats);

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

    private static DateTimeOffset Utc(DateTime value) => new(value, TimeSpan.Zero);
}
