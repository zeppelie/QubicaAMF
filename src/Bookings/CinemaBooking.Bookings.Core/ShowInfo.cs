namespace CinemaBooking.Bookings.Core;

public sealed record ShowInfo(int ShowId, DateTime StartsAt, IReadOnlyList<HallSeat> Seats);