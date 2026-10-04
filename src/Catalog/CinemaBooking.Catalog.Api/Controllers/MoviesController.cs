using CinemaBooking.Catalog.Core;
using CinemaBooking.Catalog.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Catalog.Api.Controllers;

[ApiController]
[Route("movies")]
public sealed class MoviesController(ICatalogRepository repository) : ControllerBase
{
    /// <summary>Lists the movies in the catalog.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<MovieResponse>> List(CancellationToken cancellationToken)
    {
        var movies = await repository.ListMoviesAsync(cancellationToken);
        return [.. movies.Select(movie => movie.ToResponse())];
    }

    /// <summary>Adds a movie to the catalog.</summary>
    [HttpPost]
    [ProducesResponseType<MovieResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = new Movie { Title = request.Title.Trim(), DurationMinutes = request.DurationMinutes };
        await repository.AddMovieAsync(movie, cancellationToken);
        return Created($"/movies/{movie.MovieId}", movie.ToResponse());
    }
}
