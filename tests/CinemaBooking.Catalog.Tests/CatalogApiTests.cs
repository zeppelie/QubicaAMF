using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Catalog.Api;
using CinemaBooking.Catalog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaBooking.Catalog.Tests;

[Trait("Category", "Integration")]
public class CatalogApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Lists_the_halls()
    {
        var halls = await _client.GetFromJsonAsync<List<HallResponse>>("/halls");

        Assert.Contains(halls!, hall => hall.Name == "Hall 1");
    }

    [Fact]
    public async Task Lists_the_seats_of_the_hall_where_a_show_takes_place()
    {
        var (showId, seatsInHall) = await InDatabase(async db =>
        {
            var show = await db.Shows.FirstAsync();
            return (show.ShowId, await db.Seats.CountAsync(seat => seat.HallId == show.HallId));
        });

        var seats = await _client.GetFromJsonAsync<List<SeatResponse>>($"/shows/{showId}/seats");

        Assert.Equal(seatsInHall, seats!.Count);
    }

    [Fact]
    public async Task Answers_not_found_for_an_unknown_show()
    {
        var response = await _client.GetAsync("/shows/0");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_movie_without_title()
    {
        var response = await _client.PostAsJsonAsync("/movies", new CreateMovieRequest("", 90));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Schedules_a_show_and_refuses_a_second_one_in_the_same_slot()
    {
        var (movieId, hallId) = await InDatabase(async db =>
            ((await db.Movies.FirstAsync()).MovieId, (await db.Halls.FirstAsync()).HallId));
        var startsAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(Random.Shared.Next(500_000));
        var request = new CreateShowRequest(movieId, hallId, startsAt);

        var first = await _client.PostAsJsonAsync("/shows", request);
        try
        {
            var second = await _client.PostAsJsonAsync("/shows", request);

            Assert.Equal(HttpStatusCode.Created, first.StatusCode);
            Assert.Equal(startsAt, (await first.Content.ReadFromJsonAsync<ShowResponse>())!.StartsAt);
            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }
        finally
        {
            await InDatabase(db => db.Shows.Where(show => show.StartsAt == startsAt.UtcDateTime).ExecuteDeleteAsync());
        }
    }

    private async Task<T> InDatabase<T>(Func<CatalogDbContext, Task<T>> query)
    {
        using var scope = factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<CatalogDbContext>());
    }
}
