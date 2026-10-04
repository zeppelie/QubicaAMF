namespace CinemaBooking.Bookings.Core;

public sealed record SeatRequest(int ShowId, IReadOnlyList<int>? SeatIds, int? Quantity)
{
    /// <summary>A request names its seats or says how many it wants, never both.</summary>
    public bool IsValid => SeatIds is { Count: > 0 } ^ Quantity is > 0;
}