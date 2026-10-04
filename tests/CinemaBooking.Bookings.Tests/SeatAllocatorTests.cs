using CinemaBooking.Bookings.Core;

namespace CinemaBooking.Bookings.Tests;

public class SeatAllocatorTests
{
    private static readonly IReadOnlyList<HallSeat> Hall = Halls.WithRows("AB", seatsPerRow: 4);

    [Fact]
    public void Picks_the_first_seats_of_an_empty_hall()
    {
        var seats = SeatAllocator.Pick(Hall, Taken(), quantity: 3);

        Assert.Equal(["A1", "A2", "A3"], Names(seats));
    }

    [Fact]
    public void Keeps_the_group_together_by_moving_to_a_row_with_enough_room()
    {
        var seats = SeatAllocator.Pick(Hall, Taken("A2", "A3"), quantity: 2);

        Assert.Equal(["B1", "B2"], Names(seats));
    }

    [Fact]
    public void Splits_the_group_when_no_row_has_enough_neighbouring_seats()
    {
        var seats = SeatAllocator.Pick(Hall, Taken("A2", "A4", "B2", "B4"), quantity: 2);

        Assert.Equal(["A1", "A3"], Names(seats));
    }

    [Fact]
    public void Gives_up_when_there_are_not_enough_free_seats()
    {
        var seats = SeatAllocator.Pick(Hall, Taken("A1", "A2", "A3", "A4", "B1", "B2"), quantity: 3);

        Assert.Null(seats);
    }

    private static HashSet<int> Taken(params string[] names) =>
        [.. Hall.Where(seat => names.Contains(seat.RowLabel + seat.SeatNumber)).Select(seat => seat.SeatId)];

    private static IEnumerable<string> Names(IReadOnlyList<HallSeat>? seats) =>
        seats!.Select(seat => seat.RowLabel + seat.SeatNumber);
}
