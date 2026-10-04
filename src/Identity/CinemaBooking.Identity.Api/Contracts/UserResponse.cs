namespace CinemaBooking.Identity.Api.Contracts;

public sealed record UserResponse(int UserId, string UserName, string Role);