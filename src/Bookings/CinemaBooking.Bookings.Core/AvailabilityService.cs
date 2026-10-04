namespace CinemaBooking.Bookings.Core;

public sealed class AvailabilityService(IShowCatalog catalog, IBookingRepository repository) : IAvailabilityService
{
    public async Task<ShowAvailability?> GetAsync(int showId, CancellationToken cancellationToken)
    {
        var show = await catalog.FindShowAsync(showId, cancellationToken);
        if (show is null)
            return null;

        var taken = await repository.ListTakenSeatIdsAsync(showId, cancellationToken);
        return new ShowAvailability(
            showId,
            [.. show.Seats.Select(seat => new SeatAvailability(seat, !taken.Contains(seat.SeatId)))]);
    }
}
