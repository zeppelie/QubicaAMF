namespace CinemaBooking.Bookings.Core;

/// <summary>The seats of one show while a booking is being put together.</summary>
internal sealed class ShowSeating(ShowInfo show, IEnumerable<int> takenSeatIds)
{
    private readonly HashSet<int> _taken = [.. takenSeatIds];

    public ShowInfo Show => show;

    public BookingError? TryTake(SeatRequest request, out IReadOnlyList<int> seatIds)
    {
        var error = request.SeatIds is null
            ? Assign(request.Quantity!.Value, out seatIds)
            : Choose(request.SeatIds, out seatIds);

        // Remembered here so a later item of the same booking cannot get these seats again.
        if (error is null)
            _taken.UnionWith(seatIds);

        return error;
    }

    private BookingError? Assign(int quantity, out IReadOnlyList<int> seatIds)
    {
        var seats = SeatAllocator.Pick(show.Seats, _taken, quantity);
        seatIds = seats is null ? [] : [.. seats.Select(seat => seat.SeatId)];
        return seats is null ? BookingError.NotEnoughSeats : null;
    }

    private BookingError? Choose(IReadOnlyList<int> chosen, out IReadOnlyList<int> seatIds)
    {
        seatIds = [.. chosen.Distinct()];
        if (seatIds.Any(seatId => show.Seats.All(seat => seat.SeatId != seatId)))
            return BookingError.SeatNotFound;

        return seatIds.Any(_taken.Contains) ? BookingError.SeatTaken : null;
    }
}