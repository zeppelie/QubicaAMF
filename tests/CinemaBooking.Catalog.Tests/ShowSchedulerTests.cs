using CinemaBooking.Catalog.Core;
using CinemaBooking.Catalog.Core.Entities;
using CinemaBooking.Tests.Shared;

namespace CinemaBooking.Catalog.Tests;

public class ShowSchedulerTests
{
    private const int HallId = 1;
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Tonight = Now.AddHours(8);
    private static readonly Movie Movie = new() { MovieId = 7, Title = "Blade Runner", DurationMinutes = 120 };

    private readonly InMemoryCatalog _catalog = new(Movie, HallId);
    private readonly ShowScheduler _scheduler;

    public ShowSchedulerTests() => _scheduler = new ShowScheduler(_catalog, new FixedClock(Now));

    [Fact]
    public async Task Schedules_a_show_when_the_hall_is_free()
    {
        var result = await _scheduler.ScheduleAsync(Movie.MovieId, HallId, Tonight, CancellationToken.None);

        Assert.Null(result.Error);
        Assert.Equal(Tonight, Assert.Single(_catalog.Shows).StartsAt);
    }

    [Fact]
    public async Task Refuses_a_show_that_starts_in_the_past()
    {
        var result = await _scheduler.ScheduleAsync(Movie.MovieId, HallId, Now.AddMinutes(-1), CancellationToken.None);

        Assert.Equal(ScheduleShowError.StartsInThePast, result.Error);
    }

    [Fact]
    public async Task Refuses_an_unknown_movie()
    {
        var result = await _scheduler.ScheduleAsync(movieId: 99, HallId, Tonight, CancellationToken.None);

        Assert.Equal(ScheduleShowError.MovieNotFound, result.Error);
    }

    [Fact]
    public async Task Refuses_an_unknown_hall()
    {
        var result = await _scheduler.ScheduleAsync(Movie.MovieId, hallId: 99, Tonight, CancellationToken.None);

        Assert.Equal(ScheduleShowError.HallNotFound, result.Error);
    }

    [Theory]
    [InlineData(-60)]
    [InlineData(0)]
    [InlineData(119)]
    public async Task Refuses_a_show_that_overlaps_another_one_in_the_same_hall(int minutesAfterTheOtherStarts)
    {
        await _scheduler.ScheduleAsync(Movie.MovieId, HallId, Tonight, CancellationToken.None);

        var result = await _scheduler.ScheduleAsync(
            Movie.MovieId, HallId, Tonight.AddMinutes(minutesAfterTheOtherStarts), CancellationToken.None);

        Assert.Equal(ScheduleShowError.HallBusy, result.Error);
        Assert.Single(_catalog.Shows);
    }

    [Fact]
    public async Task Accepts_a_show_that_starts_when_the_previous_one_ends()
    {
        await _scheduler.ScheduleAsync(Movie.MovieId, HallId, Tonight, CancellationToken.None);

        var result = await _scheduler.ScheduleAsync(
            Movie.MovieId, HallId, Tonight.AddMinutes(Movie.DurationMinutes), CancellationToken.None);

        Assert.Null(result.Error);
        Assert.Equal(2, _catalog.Shows.Count);
    }

    [Fact]
    public async Task Reports_a_busy_hall_when_someone_else_takes_the_slot_first()
    {
        _catalog.SomeoneElseTakesTheSlot = true;

        var result = await _scheduler.ScheduleAsync(Movie.MovieId, HallId, Tonight, CancellationToken.None);

        Assert.Equal(ScheduleShowError.HallBusy, result.Error);
    }

    private sealed class InMemoryCatalog(Movie movie, int hallId) : ICatalogRepository
    {
        public List<Show> Shows { get; } = [];

        public bool SomeoneElseTakesTheSlot { get; set; }

        public Task<bool> HallExistsAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(id == hallId);

        public Task<Movie?> FindMovieAsync(int movieId, CancellationToken cancellationToken) =>
            Task.FromResult(movieId == movie.MovieId ? movie : null);

        public Task<IReadOnlyList<Show>> ListShowsAsync(DateTime from, DateTime to, int? hall, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Show>>(
                [.. Shows.Where(show => show.StartsAt >= from && show.StartsAt < to && (hall == null || show.HallId == hall))]);

        public Task<bool> TryAddShowAsync(Show show, CancellationToken cancellationToken)
        {
            if (SomeoneElseTakesTheSlot)
                return Task.FromResult(false);

            show.Movie = movie;
            Shows.Add(show);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<Hall>> ListHallsAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Seat>> ListSeatsAsync(int id, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyList<Movie>> ListMoviesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task AddMovieAsync(Movie newMovie, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<Show?> FindShowAsync(int showId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
