using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Bookings.Api;
using CinemaBooking.Security;
using CinemaBooking.Tests.Shared;

namespace CinemaBooking.Bookings.Tests;

[Trait("Category", "Integration")]
public class BookingsApiTests(BookingsApiFactory factory) : IClassFixture<BookingsApiFactory>
{
    private readonly int _showId = factory.NextShowId();

    [Fact]
    public async Task A_booked_seat_is_no_longer_available()
    {
        await Book(factory.Alice, new BookingItemRequest(_showId, [2], Quantity: null));

        var availability = await factory.CreateClient()
            .GetFromJsonAsync<ShowAvailabilityResponse>($"/shows/{_showId}/availability");

        Assert.Equal(5, availability!.FreeSeats);
        Assert.False(availability.Seats.Single(seat => seat.SeatId == 2).IsFree);
    }

    [Fact]
    public async Task Books_the_requested_quantity_of_seats()
    {
        var response = await Book(factory.Alice, new BookingItemRequest(_showId, SeatIds: null, Quantity: 3));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(3, (await response.Content.ReadFromJsonAsync<BookingResponse>())!.Seats.Count);
    }

    [Fact]
    public async Task Books_several_shows_at_once()
    {
        var otherShowId = factory.NextShowId();

        var response = await Book(
            factory.Alice,
            new BookingItemRequest(_showId, [1, 2], Quantity: null),
            new BookingItemRequest(otherShowId, SeatIds: null, Quantity: 1));

        var booking = await response.Content.ReadFromJsonAsync<BookingResponse>();
        Assert.Equal([_showId, _showId, otherShowId], booking!.Seats.Select(seat => seat.ShowId));
    }

    [Fact]
    public async Task Refuses_a_seat_that_another_user_already_booked()
    {
        await Book(factory.Alice, new BookingItemRequest(_showId, [1], Quantity: null));

        var response = await Book(factory.Bob, new BookingItemRequest(_showId, [1], Quantity: null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Only_one_of_many_simultaneous_requests_gets_the_seat()
    {
        var attempts = Enumerable.Range(0, 8)
            .Select(_ => Book(factory.Alice, new BookingItemRequest(_showId, [1], Quantity: null)));

        var responses = await Task.WhenAll(attempts);

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Equal(7, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task Answers_not_found_for_an_unknown_show()
    {
        var response = await Book(factory.Alice, new BookingItemRequest(ShowId: 1, SeatIds: null, Quantity: 1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_booking_without_items()
    {
        var response = await Book(factory.Alice);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_a_booking_without_a_token()
    {
        var request = new CreateBookingRequest([new BookingItemRequest(_showId, SeatIds: null, Quantity: 1)]);

        var response = await factory.CreateClient().PostAsJsonAsync("/bookings", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_cancelled_booking_frees_its_seats_for_someone_else()
    {
        var booking = await (await Book(factory.Alice, new BookingItemRequest(_showId, [1], Quantity: null)))
            .Content.ReadFromJsonAsync<BookingResponse>();

        var cancelled = await ClientOf(factory.Alice).DeleteAsync($"/bookings/{booking!.BookingId}");
        var cancelledAgain = await ClientOf(factory.Alice).DeleteAsync($"/bookings/{booking.BookingId}");
        var rebooked = await Book(factory.Bob, new BookingItemRequest(_showId, [1], Quantity: null));

        Assert.Equal(HttpStatusCode.NoContent, cancelled.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, cancelledAgain.StatusCode);
        Assert.Equal(HttpStatusCode.Created, rebooked.StatusCode);
    }

    [Fact]
    public async Task A_user_sees_and_cancels_only_their_own_bookings()
    {
        var booking = await (await Book(factory.Alice, new BookingItemRequest(_showId, [1], Quantity: null)))
            .Content.ReadFromJsonAsync<BookingResponse>();

        var seenByBob = await ClientOf(factory.Bob).GetAsync($"/bookings/{booking!.BookingId}");
        var cancelledByBob = await ClientOf(factory.Bob).DeleteAsync($"/bookings/{booking.BookingId}");
        var listOfAlice = await ClientOf(factory.Alice).GetFromJsonAsync<List<BookingResponse>>("/bookings");

        Assert.Equal(HttpStatusCode.NotFound, seenByBob.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, cancelledByBob.StatusCode);
        Assert.Contains(listOfAlice!, mine => mine.BookingId == booking.BookingId);
    }

    private Task<HttpResponseMessage> Book(int userId, params BookingItemRequest[] items) =>
        ClientOf(userId).PostAsJsonAsync("/bookings", new CreateBookingRequest(items));

    private HttpClient ClientOf(int userId) => factory.CreateClientFor(userId, Roles.Customer);
}
