namespace CinemaBooking.Bookings.Core;

/// <summary>Gives access to the shows and seats owned by the Catalog service.</summary>
public interface IShowCatalog
{
    /// <summary>Returns the show with the seats of its hall, or null when the show does not exist.</summary>
    Task<ShowInfo?> FindShowAsync(int showId, CancellationToken cancellationToken);
}

public sealed record HallSeat(int SeatId, string RowLabel, int SeatNumber);

public sealed record ShowInfo(int ShowId, DateTime StartsAt, IReadOnlyList<HallSeat> Seats);
