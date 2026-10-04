namespace CinemaBooking.Bookings.Api.Contracts;

public sealed record SeatAvailabilityResponse(int SeatId, string RowLabel, int SeatNumber, bool IsFree);
