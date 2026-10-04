using CinemaBooking.Catalog.Core;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Catalog.Api.Controllers;

[ApiController]
[Route("halls")]
public sealed class HallsController(ICatalogRepository repository) : ControllerBase
{
    /// <summary>Lists the halls of the cinema.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<HallResponse>> List(CancellationToken cancellationToken)
    {
        var halls = await repository.ListHallsAsync(cancellationToken);
        return [.. halls.Select(hall => hall.ToResponse())];
    }
}
