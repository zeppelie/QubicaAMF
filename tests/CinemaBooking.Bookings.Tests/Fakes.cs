using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Tests;

internal sealed class FakeShowCatalog(params ShowInfo[] shows) : IShowCatalog
{
    public int Lookups { get; private set; }

    public Task<ShowInfo?> FindShowAsync(int showId, CancellationToken cancellationToken)
    {
        Lookups++;
        return Task.FromResult(shows.FirstOrDefault(show => show.ShowId == showId));
    }
}

internal sealed class InMemoryBookings : IBookingRepository
{
    public List<Booking> Bookings { get; } = [];

    public bool SomeoneElseWinsTheRace { get; set; }

    public Task<IReadOnlySet<int>> ListTakenSeatIdsAsync(int showId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlySet<int>>(Bookings
            .SelectMany(booking => booking.BookedSeats)
            .Where(seat => seat.ShowId == showId && seat.CancelledAt is null)
            .Select(seat => seat.SeatId)
            .ToHashSet());

    public Task<bool> TryAddAsync(Booking booking, CancellationToken cancellationToken)
    {
        if (SomeoneElseWinsTheRace)
            return Task.FromResult(false);

        booking.BookingId = Bookings.Count + 1;
        Bookings.Add(booking);
        return Task.FromResult(true);
    }

    public Task<Booking?> FindAsync(int bookingId, CancellationToken cancellationToken) =>
        Task.FromResult(Bookings.FirstOrDefault(booking => booking.BookingId == bookingId));

    public Task<IReadOnlyList<Booking>> ListByUserAsync(int userId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Booking>>([.. Bookings.Where(booking => booking.UserId == userId)]);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal static class Halls
{
    public static IReadOnlyList<HallSeat> WithRows(string rows, int seatsPerRow)
    {
        var seats = new List<HallSeat>();
        foreach (var row in rows)
            for (var number = 1; number <= seatsPerRow; number++)
                seats.Add(new HallSeat(seats.Count + 1, row.ToString(), number));
        return seats;
    }
}
