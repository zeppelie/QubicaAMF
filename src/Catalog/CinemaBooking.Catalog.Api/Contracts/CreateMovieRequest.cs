using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Catalog.Api.Contracts;

public sealed record CreateMovieRequest(
    [Required, StringLength(200)] string Title,
    [Range(1, 600)] int DurationMinutes);