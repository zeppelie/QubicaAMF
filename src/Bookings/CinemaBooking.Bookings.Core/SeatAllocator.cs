namespace CinemaBooking.Bookings.Core;

public static class SeatAllocator
{
    /// <summary>Picks free seats next to each other when possible, otherwise the first free ones; null when there are not enough.</summary>
    public static IReadOnlyList<HallSeat>? Pick(IReadOnlyList<HallSeat> seats, IReadOnlySet<int> takenSeatIds, int quantity)
    {
        var free = seats
            .Where(seat => !takenSeatIds.Contains(seat.SeatId))
            .OrderBy(seat => seat.RowLabel, StringComparer.Ordinal)
            .ThenBy(seat => seat.SeatNumber)
            .ToList();

        if (free.Count < quantity)
            return null;

        return FindNeighbours(free, quantity) ?? free.GetRange(0, quantity);
    }

    private static List<HallSeat>? FindNeighbours(List<HallSeat> free, int quantity)
    {
        for (var start = 0; start + quantity <= free.Count; start++)
        {
            var first = free[start];
            var last = free[start + quantity - 1];
            if (first.RowLabel == last.RowLabel && last.SeatNumber - first.SeatNumber == quantity - 1)
                return free.GetRange(start, quantity);
        }

        return null;
    }
}
