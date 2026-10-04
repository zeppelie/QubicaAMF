using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

public sealed class BookingService(IShowCatalog catalog, IBookingRepository repository, TimeProvider clock) : IBookingService
{
    public async Task<BookingResult> BookAsync(int userId, IReadOnlyList<SeatRequest> requests, CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
            return BookingResult.Failed(BookingError.InvalidRequest);

        var now = clock.GetUtcNow().UtcDateTime;
        var booking = new Booking { UserId = userId, CreatedAt = now };
        var takenByShow = new Dictionary<int, HashSet<int>>();

        foreach (var request in requests)
        {
            if (!AsksForSeatsOrQuantity(request))
                return BookingResult.Failed(BookingError.InvalidRequest, request.ShowId);

            var show = await catalog.FindShowAsync(request.ShowId, cancellationToken);
            if (show is null)
                return BookingResult.Failed(BookingError.ShowNotFound, request.ShowId);
            if (show.StartsAt <= now)
                return BookingResult.Failed(BookingError.ShowAlreadyStarted, show.ShowId);

            if (!takenByShow.TryGetValue(show.ShowId, out var taken))
                takenByShow[show.ShowId] = taken = [.. await repository.ListTakenSeatIdsAsync(show.ShowId, cancellationToken)];

            var (seatIds, error) = request.SeatIds is null
                ? AssignSeats(show, taken, request.Quantity!.Value)
                : CheckChosenSeats(show, taken, request.SeatIds);
            if (error is not null)
                return BookingResult.Failed(error.Value, show.ShowId);

            foreach (var seatId in seatIds)
            {
                taken.Add(seatId);
                booking.BookedSeats.Add(new BookedSeat { ShowId = show.ShowId, SeatId = seatId });
            }
        }

        return await repository.TryAddAsync(booking, cancellationToken)
            ? BookingResult.Booked(booking)
            : BookingResult.Failed(BookingError.SeatTaken);
    }

    public async Task<CancelBookingError?> CancelAsync(int userId, int bookingId, CancellationToken cancellationToken)
    {
        var booking = await repository.FindAsync(bookingId, cancellationToken);
        if (booking is null || booking.UserId != userId)
            return CancelBookingError.BookingNotFound;
        if (booking.IsCancelled)
            return CancelBookingError.AlreadyCancelled;

        booking.Cancel(clock.GetUtcNow().UtcDateTime);
        await repository.UpdateAsync(booking, cancellationToken);
        return null;
    }

    private static bool AsksForSeatsOrQuantity(SeatRequest request) =>
        request.SeatIds is { Count: > 0 } ^ request.Quantity is > 0;

    private static (IReadOnlyList<int> SeatIds, BookingError? Error) AssignSeats(ShowInfo show, HashSet<int> taken, int quantity)
    {
        var seats = SeatAllocator.Pick(show.Seats, taken, quantity);
        return seats is null
            ? ([], BookingError.NotEnoughSeats)
            : ([.. seats.Select(seat => seat.SeatId)], null);
    }

    private static (IReadOnlyList<int> SeatIds, BookingError? Error) CheckChosenSeats(ShowInfo show, HashSet<int> taken, IReadOnlyList<int> chosen)
    {
        var seatIds = chosen.Distinct().ToList();
        if (seatIds.Any(seatId => show.Seats.All(seat => seat.SeatId != seatId)))
            return ([], BookingError.SeatNotFound);
        if (seatIds.Any(taken.Contains))
            return ([], BookingError.SeatTaken);

        return (seatIds, null);
    }
}
