using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Identity.Api.Contracts;

public sealed record LoginRequest([Required] string UserName, [Required] string Password);