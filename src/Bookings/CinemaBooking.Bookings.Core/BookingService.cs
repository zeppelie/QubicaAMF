using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Core;

public sealed class BookingService(IShowCatalog catalog, IBookingRepository repository, TimeProvider clock) : IBookingService
{
    public async Task<BookingResult> BookAsync(int userId, IReadOnlyList<SeatRequest> requests, CancellationToken cancellationToken)
    {
        if (requests.Count == 0)
            return BookingResult.Failed(BookingError.InvalidRequest);

        var booking = new Booking { UserId = userId, CreatedAt = clock.GetUtcNow().UtcDateTime };
        var seatings = new Dictionary<int, ShowSeating>();

        foreach (var request in requests)
        {
            var failure = await AddSeatsAsync(booking, request, seatings, cancellationToken);
            if (failure is not null)
                return failure;
        }

        return await repository.TryAddAsync(booking, cancellationToken)
            ? BookingResult.Booked(booking)
            : BookingResult.Failed(BookingError.SeatTaken);
    }

    public async Task<Booking?> FindAsync(int userId, int bookingId, CancellationToken cancellationToken)
    {
        var booking = await repository.FindAsync(bookingId, cancellationToken);
        return booking?.UserId == userId ? booking : null;
    }

    public Task<IReadOnlyList<Booking>> ListAsync(int userId, CancellationToken cancellationToken) =>
        repository.ListByUserAsync(userId, cancellationToken);

    public async Task<CancelBookingError?> CancelAsync(int userId, int bookingId, CancellationToken cancellationToken)
    {
        var booking = await FindAsync(userId, bookingId, cancellationToken);
        if (booking is null)
            return CancelBookingError.BookingNotFound;
        if (booking.IsCancelled)
            return CancelBookingError.AlreadyCancelled;

        booking.Cancel(clock.GetUtcNow().UtcDateTime);
        await repository.SaveChangesAsync(cancellationToken);
        return null;
    }

    private async Task<BookingResult?> AddSeatsAsync(
        Booking booking, SeatRequest request, Dictionary<int, ShowSeating> seatings, CancellationToken cancellationToken)
    {
        if (!request.IsValid)
            return BookingResult.Failed(BookingError.InvalidRequest, request.ShowId);

        var seating = await SeatingOfAsync(request.ShowId, seatings, cancellationToken);
        if (seating is null)
            return BookingResult.Failed(BookingError.ShowNotFound, request.ShowId);
        if (seating.Show.StartsAt <= booking.CreatedAt)
            return BookingResult.Failed(BookingError.ShowAlreadyStarted, request.ShowId);

        var error = seating.TryTake(request, out var seatIds);
        if (error is not null)
            return BookingResult.Failed(error.Value, request.ShowId);

        foreach (var seatId in seatIds)
            booking.BookedSeats.Add(new BookedSeat { ShowId = request.ShowId, SeatId = seatId });

        return null;
    }

    private async Task<ShowSeating?> SeatingOfAsync(
        int showId, Dictionary<int, ShowSeating> seatings, CancellationToken cancellationToken)
    {
        if (seatings.TryGetValue(showId, out var known))
            return known;

        var show = await catalog.FindShowAsync(showId, cancellationToken);
        if (show is null)
            return null;

        var taken = await repository.ListTakenSeatIdsAsync(showId, cancellationToken);
        return seatings[showId] = new ShowSeating(show, taken);
    }
}