using CinemaBooking.Bookings.Core;
using CinemaBooking.Bookings.Core.Entities;

namespace CinemaBooking.Bookings.Tests;

public class AvailabilityServiceTests
{
    private const int ShowId = 10;
    private static readonly IReadOnlyList<HallSeat> Hall = Halls.WithRows("A", seatsPerRow: 3);

    private readonly InMemoryBookings _bookings = new();
    private readonly AvailabilityService _service;

    public AvailabilityServiceTests() =>
        _service = new AvailabilityService(new FakeShowCatalog(new ShowInfo(ShowId, DateTime.UtcNow, Hall)), _bookings);

    [Fact]
    public async Task Marks_the_booked_seats_as_taken()
    {
        _bookings.Bookings.Add(new Booking { BookedSeats = [new BookedSeat { ShowId = ShowId, SeatId = 2 }] });

        var availability = await _service.GetAsync(ShowId, CancellationToken.None);

        Assert.Equal([true, false, true], availability!.Seats.Select(seat => seat.IsFree));
        Assert.Equal(2, availability.FreeSeats);
    }

    [Fact]
    public async Task Counts_the_seats_of_a_cancelled_booking_as_free()
    {
        var booking = new Booking { BookedSeats = [new BookedSeat { ShowId = ShowId, SeatId = 2 }] };
        booking.Cancel(DateTime.UtcNow);
        _bookings.Bookings.Add(booking);

        var availability = await _service.GetAsync(ShowId, CancellationToken.None);

        Assert.Equal(3, availability!.FreeSeats);
    }

    [Fact]
    public async Task Returns_nothing_for_an_unknown_show()
    {
        Assert.Null(await _service.GetAsync(showId: 999, CancellationToken.None));
    }
}
