using CinemaBooking.Bookings.Core;
using CinemaBooking.Tests.Shared;

namespace CinemaBooking.Bookings.Tests;

public class BookingServiceTests
{
    private const int Alice = 1;
    private const int Bob = 2;
    private const int Matinee = 10;
    private const int Evening = 11;
    private const int AlreadyStarted = 12;
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly IReadOnlyList<HallSeat> Hall = Halls.WithRows("AB", seatsPerRow: 3);

    private readonly InMemoryBookings _bookings = new();
    private readonly FakeShowCatalog _catalog = new(
        new ShowInfo(Matinee, Now.AddHours(3), Hall),
        new ShowInfo(Evening, Now.AddHours(8), Hall),
        new ShowInfo(AlreadyStarted, Now.AddMinutes(-10), Hall));
    private readonly BookingService _service;

    public BookingServiceTests() => _service = new BookingService(_catalog, _bookings, new FixedClock(Now));

    [Fact]
    public async Task Books_the_seats_chosen_by_the_user()
    {
        var result = await Book(Alice, Seats(Matinee, 2, 3));

        Assert.Null(result.Error);
        Assert.Equal([2, 3], result.Booking!.BookedSeats.Select(seat => seat.SeatId));
        Assert.Equal(Alice, result.Booking.UserId);
    }

    [Fact]
    public async Task Assigns_seats_when_the_user_only_asks_for_a_quantity()
    {
        await Book(Bob, Seats(Matinee, 2));

        var result = await Book(Alice, Quantity(Matinee, 2));

        Assert.Null(result.Error);
        Assert.Equal([4, 5], result.Booking!.BookedSeats.Select(seat => seat.SeatId));
    }

    [Fact]
    public async Task Books_several_shows_in_a_single_booking()
    {
        var result = await Book(Alice, Seats(Matinee, 1), Quantity(Evening, 2));

        Assert.Null(result.Error);
        Assert.Equal([Matinee, Evening, Evening], result.Booking!.BookedSeats.Select(seat => seat.ShowId));
        Assert.Single(_bookings.Bookings);
    }

    [Fact]
    public async Task Never_assigns_the_same_seat_twice_within_one_booking()
    {
        var result = await Book(Alice, Quantity(Matinee, 2), Quantity(Matinee, 2));

        Assert.Equal(4, result.Booking!.BookedSeats.Select(seat => seat.SeatId).Distinct().Count());
    }

    [Fact]
    public async Task Asks_the_catalog_only_once_for_a_show_that_appears_in_several_items()
    {
        await Book(Alice, Seats(Matinee, 1), Seats(Matinee, 2), Quantity(Matinee, 1));

        Assert.Equal(1, _catalog.Lookups);
    }

    [Fact]
    public async Task Refuses_a_seat_that_is_already_taken()
    {
        await Book(Bob, Seats(Matinee, 1));

        var result = await Book(Alice, Seats(Matinee, 1, 2));

        Assert.Equal(BookingError.SeatTaken, result.Error);
        Assert.Equal(Matinee, result.ShowId);
    }

    [Fact]
    public async Task Allows_the_same_seat_in_a_different_show()
    {
        await Book(Bob, Seats(Matinee, 1));

        var result = await Book(Alice, Seats(Evening, 1));

        Assert.Null(result.Error);
    }

    [Fact]
    public async Task Refuses_when_the_show_has_not_enough_free_seats()
    {
        var result = await Book(Alice, Quantity(Matinee, Hall.Count + 1));

        Assert.Equal(BookingError.NotEnoughSeats, result.Error);
    }

    [Fact]
    public async Task Refuses_a_seat_that_is_not_in_the_hall()
    {
        var result = await Book(Alice, Seats(Matinee, 999));

        Assert.Equal(BookingError.SeatNotFound, result.Error);
    }

    [Fact]
    public async Task Refuses_an_unknown_show()
    {
        var result = await Book(Alice, Quantity(showId: 999, 1));

        Assert.Equal(BookingError.ShowNotFound, result.Error);
    }

    [Fact]
    public async Task Refuses_a_show_that_has_already_started()
    {
        var result = await Book(Alice, Quantity(AlreadyStarted, 1));

        Assert.Equal(BookingError.ShowAlreadyStarted, result.Error);
    }

    [Fact]
    public async Task Refuses_a_request_without_seats_and_without_quantity()
    {
        var result = await Book(Alice, new SeatRequest(Matinee, SeatIds: null, Quantity: null));

        Assert.Equal(BookingError.InvalidRequest, result.Error);
    }

    [Fact]
    public async Task Refuses_a_request_with_both_seats_and_quantity()
    {
        var result = await Book(Alice, new SeatRequest(Matinee, [1], 2));

        Assert.Equal(BookingError.InvalidRequest, result.Error);
    }

    [Fact]
    public async Task Refuses_an_empty_booking()
    {
        var result = await Book(Alice);

        Assert.Equal(BookingError.InvalidRequest, result.Error);
    }

    [Fact]
    public async Task Books_nothing_when_one_of_the_shows_cannot_be_satisfied()
    {
        var result = await Book(Alice, Seats(Matinee, 1), Seats(Evening, 999));

        Assert.Equal(BookingError.SeatNotFound, result.Error);
        Assert.Empty(_bookings.Bookings);
    }

    [Fact]
    public async Task Reports_a_taken_seat_when_someone_else_books_it_first()
    {
        _bookings.SomeoneElseWinsTheRace = true;

        var result = await Book(Alice, Seats(Matinee, 1));

        Assert.Equal(BookingError.SeatTaken, result.Error);
    }

    [Fact]
    public async Task Cancelling_frees_the_seats()
    {
        var booking = (await Book(Alice, Seats(Matinee, 1, 2))).Booking!;

        var error = await _service.CancelAsync(Alice, booking.BookingId, CancellationToken.None);

        Assert.Null(error);
        Assert.Equal(Now, booking.CancelledAt);
        Assert.Null((await Book(Bob, Seats(Matinee, 1, 2))).Error);
    }

    [Fact]
    public async Task Cannot_cancel_the_booking_of_another_user()
    {
        var booking = (await Book(Alice, Seats(Matinee, 1))).Booking!;

        var error = await _service.CancelAsync(Bob, booking.BookingId, CancellationToken.None);

        Assert.Equal(CancelBookingError.BookingNotFound, error);
        Assert.False(booking.IsCancelled);
    }

    [Fact]
    public async Task Cannot_cancel_once_one_of_the_shows_has_started()
    {
        var booking = (await Book(Alice, Seats(Matinee, 1), Seats(Evening, 1))).Booking!;
        var duringTheMatinee = new BookingService(_catalog, _bookings, new FixedClock(Now.AddHours(4)));

        var error = await duringTheMatinee.CancelAsync(Alice, booking.BookingId, CancellationToken.None);

        Assert.Equal(CancelBookingError.ShowAlreadyStarted, error);
        Assert.False(booking.IsCancelled);
    }

    [Fact]
    public async Task Shows_a_booking_only_to_the_user_who_made_it()
    {
        var booking = (await Book(Alice, Seats(Matinee, 1))).Booking!;

        Assert.Same(booking, await _service.FindAsync(Alice, booking.BookingId, CancellationToken.None));
        Assert.Null(await _service.FindAsync(Bob, booking.BookingId, CancellationToken.None));
    }

    [Fact]
    public async Task Cannot_cancel_a_booking_twice()
    {
        var booking = (await Book(Alice, Seats(Matinee, 1))).Booking!;
        await _service.CancelAsync(Alice, booking.BookingId, CancellationToken.None);

        var error = await _service.CancelAsync(Alice, booking.BookingId, CancellationToken.None);

        Assert.Equal(CancelBookingError.AlreadyCancelled, error);
    }

    private Task<BookingResult> Book(int userId, params SeatRequest[] requests) =>
        _service.BookAsync(userId, requests, CancellationToken.None);

    private static SeatRequest Seats(int showId, params int[] seatIds) => new(showId, seatIds, Quantity: null);

    private static SeatRequest Quantity(int showId, int quantity) => new(showId, SeatIds: null, quantity);
}
