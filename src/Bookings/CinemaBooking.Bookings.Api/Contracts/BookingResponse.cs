namespace CinemaBooking.Bookings.Api.Contracts;

public sealed record BookingResponse(
    int BookingId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CancelledAt,
    IReadOnlyList<BookedSeatResponse> Seats);
