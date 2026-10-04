using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Identity.Api.Contracts;

public sealed record RegisterRequest(
    [Required, StringLength(50, MinimumLength = 3)] string UserName,
    [Required, StringLength(100, MinimumLength = 8)] string Password);