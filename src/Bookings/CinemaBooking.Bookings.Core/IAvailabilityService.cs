namespace CinemaBooking.Bookings.Core;

/// <summary>Tells which seats of a show can still be booked.</summary>
public interface IAvailabilityService
{
    /// <summary>Returns every seat of the show marked as free or taken, or null when the show does not exist.</summary>
    Task<ShowAvailability?> GetAsync(int showId, CancellationToken cancellationToken);
}

public sealed record SeatAvailability(HallSeat Seat, bool IsFree);

public sealed record ShowAvailability(int ShowId, IReadOnlyList<SeatAvailability> Seats)
{
    public int FreeSeats => Seats.Count(seat => seat.IsFree);
}
