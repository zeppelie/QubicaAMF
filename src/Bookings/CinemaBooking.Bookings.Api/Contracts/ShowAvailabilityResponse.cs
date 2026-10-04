namespace CinemaBooking.Bookings.Api.Contracts;

public sealed record ShowAvailabilityResponse(int ShowId, int FreeSeats, IReadOnlyList<SeatAvailabilityResponse> Seats);
