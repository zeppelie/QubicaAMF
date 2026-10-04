using System.Net;
using System.Text;
using CinemaBooking.Bookings.Infrastructure.Catalog;

namespace CinemaBooking.Bookings.Tests;

public class CatalogShowClientTests
{
    [Fact]
    public async Task Reads_the_show_and_the_seats_of_its_hall()
    {
        var client = ClientAnswering(new()
        {
            ["/shows/7"] = """{"showId":7,"movieTitle":"Blade Runner","startsAt":"2026-10-05T21:00:00+02:00"}""",
            ["/shows/7/seats"] = """[{"seatId":1,"rowLabel":"A","seatNumber":1},{"seatId":2,"rowLabel":"A","seatNumber":2}]"""
        });

        var show = await client.FindShowAsync(7, CancellationToken.None);

        Assert.Equal(new DateTime(2026, 10, 5, 19, 0, 0), show!.StartsAt);
        Assert.Equal(["A1", "A2"], show.Seats.Select(seat => seat.RowLabel + seat.SeatNumber));
    }

    [Fact]
    public async Task Returns_nothing_when_the_catalog_does_not_know_the_show()
    {
        var client = ClientAnswering([]);

        Assert.Null(await client.FindShowAsync(7, CancellationToken.None));
    }

    private static CatalogShowClient ClientAnswering(Dictionary<string, string> jsonByPath) =>
        new(new HttpClient(new StubCatalog(jsonByPath)) { BaseAddress = new Uri("http://catalog") });

    private sealed class StubCatalog(Dictionary<string, string> jsonByPath) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(jsonByPath.TryGetValue(request.RequestUri!.AbsolutePath, out var json)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") }
                : new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
