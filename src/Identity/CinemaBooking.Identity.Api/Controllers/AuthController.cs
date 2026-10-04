using CinemaBooking.Identity.Api.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.Identity.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAccountService accounts) : ControllerBase
{
    /// <summary>Registers a new customer.</summary>
    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var account = await accounts.RegisterAsync(request.UserName, request.Password, cancellationToken);
        return account is null
            ? Problem($"User name '{request.UserName}' is already taken.", statusCode: StatusCodes.Status409Conflict)
            : StatusCode(StatusCodes.Status201Created, new UserResponse(account.UserId, account.UserName, account.Role));
    }

    /// <summary>Signs a user in and returns the token to send to the other services.</summary>
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await accounts.LoginAsync(request.UserName, request.Password, cancellationToken);
        return token is null
            ? Problem("Wrong user name or password.", statusCode: StatusCodes.Status401Unauthorized)
            : Ok(new TokenResponse(token.Value, token.ExpiresAt));
    }
}
