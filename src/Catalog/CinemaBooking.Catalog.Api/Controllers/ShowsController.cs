using CinemaBooking.Catalog.Core;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Catalog.Api.Controllers;

[ApiController]
[Route("shows")]
public sealed class ShowsController(ICatalogRepository repository, IShowScheduler scheduler, TimeProvider clock) : ControllerBase
{
    /// <summary>Lists the shows of a given day, or all the upcoming ones when no day is given.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<ShowResponse>> List([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var from = date?.ToDateTime(TimeOnly.MinValue) ?? clock.GetUtcNow().UtcDateTime;
        var to = date?.AddDays(1).ToDateTime(TimeOnly.MinValue) ?? DateTime.MaxValue;

        var shows = await repository.ListShowsAsync(from, to, hallId: null, cancellationToken);
        return [.. shows.Select(show => show.ToResponse())];
    }

    /// <summary>Returns a single show.</summary>
    [HttpGet("{showId:int}")]
    [ProducesResponseType<ShowResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int showId, CancellationToken cancellationToken)
    {
        var show = await repository.FindShowAsync(showId, cancellationToken);
        return show is null ? ShowNotFound(showId) : Ok(show.ToResponse());
    }

    /// <summary>Lists the seats of the hall where the show takes place.</summary>
    [HttpGet("{showId:int}/seats")]
    [ProducesResponseType<IReadOnlyList<SeatResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListSeats(int showId, CancellationToken cancellationToken)
    {
        var show = await repository.FindShowAsync(showId, cancellationToken);
        if (show is null)
            return ShowNotFound(showId);

        var seats = await repository.ListSeatsAsync(show.HallId, cancellationToken);
        return Ok(seats.Select(seat => seat.ToResponse()));
    }

    /// <summary>Schedules a movie in a hall.</summary>
    [HttpPost]
    [ProducesResponseType<ShowResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateShowRequest request, CancellationToken cancellationToken)
    {
        var result = await scheduler.ScheduleAsync(request.MovieId, request.HallId, request.StartsAt.UtcDateTime, cancellationToken);

        switch (result.Error)
        {
            case ScheduleShowError.MovieNotFound:
                return Problem($"Movie {request.MovieId} does not exist.", statusCode: StatusCodes.Status404NotFound);
            case ScheduleShowError.HallNotFound:
                return Problem($"Hall {request.HallId} does not exist.", statusCode: StatusCodes.Status404NotFound);
            case ScheduleShowError.StartsInThePast:
                return Problem("A show must start in the future.", statusCode: StatusCodes.Status400BadRequest);
            case ScheduleShowError.HallBusy:
                return Problem("The hall is already taken at that time.", statusCode: StatusCodes.Status409Conflict);
        }

        var show = await repository.FindShowAsync(result.Show!.ShowId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { showId = show!.ShowId }, show.ToResponse());
    }

    private ObjectResult ShowNotFound(int showId) =>
        Problem($"Show {showId} does not exist.", statusCode: StatusCodes.Status404NotFound);
}
