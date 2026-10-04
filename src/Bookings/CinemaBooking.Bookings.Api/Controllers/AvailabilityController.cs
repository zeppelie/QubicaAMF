using CinemaBooking.Bookings.Api.Contracts;
using CinemaBooking.Bookings.Core;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Bookings.Api.Controllers;

[ApiController]
public sealed class AvailabilityController(IAvailabilityService availability) : ControllerBase
{
    /// <summary>Tells which seats of a show are free and which are taken.</summary>
    [HttpGet("shows/{showId:int}/availability")]
    [ProducesResponseType<ShowAvailabilityResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int showId, CancellationToken cancellationToken)
    {
        var result = await availability.GetAsync(showId, cancellationToken);
        return result is null
            ? Problem($"Show {showId} does not exist.", statusCode: StatusCodes.Status404NotFound)
            : Ok(result.ToResponse());
    }
}
