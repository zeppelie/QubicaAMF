namespace CinemaBooking.Bookings.Api.Contracts;

public sealed record BookingItemRequest(int ShowId, IReadOnlyList<int>? SeatIds, int? Quantity);
