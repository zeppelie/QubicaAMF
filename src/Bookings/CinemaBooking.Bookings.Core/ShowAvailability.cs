namespace CinemaBooking.Bookings.Core;

public sealed record ShowAvailability(int ShowId, IReadOnlyList<SeatAvailability> Seats)
{
    public int FreeSeats => Seats.Count(seat => seat.IsFree);
}