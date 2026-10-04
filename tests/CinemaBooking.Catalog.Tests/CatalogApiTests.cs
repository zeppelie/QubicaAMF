using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Catalog.Api;
using CinemaBooking.Catalog.Infrastructure.Persistence;
using CinemaBooking.Security;
using CinemaBooking.Tests.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaBooking.Catalog.Tests;

[Trait("Category", "Integration")]
public class CatalogApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _anonymous = factory.CreateClient();
    private readonly HttpClient _admin = factory.CreateClientFor(userId: 1, Roles.Admin);

    [Fact]
    public async Task Lists_the_halls()
    {
        var halls = await _anonymous.GetFromJsonAsync<List<HallResponse>>("/halls");

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

        var seats = await _anonymous.GetFromJsonAsync<List<SeatResponse>>($"/shows/{showId}/seats");

        Assert.Equal(seatsInHall, seats!.Count);
    }

    [Fact]
    public async Task Answers_not_found_for_an_unknown_show()
    {
        var response = await _anonymous.GetAsync("/shows/0");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_movie_without_title()
    {
        var response = await _admin.PostAsJsonAsync("/movies", new CreateMovieRequest("", 90));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Only_an_admin_can_add_movies()
    {
        var movie = new CreateMovieRequest("Metropolis", 153);
        var customer = factory.CreateClientFor(userId: 2, Roles.Customer);

        var withoutToken = await _anonymous.PostAsJsonAsync("/movies", movie);
        var asCustomer = await customer.PostAsJsonAsync("/movies", movie);

        Assert.Equal(HttpStatusCode.Unauthorized, withoutToken.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, asCustomer.StatusCode);
    }

    [Fact]
    public async Task Schedules_a_show_and_refuses_a_second_one_in_the_same_slot()
    {
        var (movieId, hallId) = await InDatabase(async db =>
            ((await db.Movies.FirstAsync()).MovieId, (await db.Halls.FirstAsync()).HallId));
        var startsAt = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(Random.Shared.Next(500_000));
        var request = new CreateShowRequest(movieId, hallId, startsAt);

        var first = await _admin.PostAsJsonAsync("/shows", request);
        try
        {
            var second = await _admin.PostAsJsonAsync("/shows", request);

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
