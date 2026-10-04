using System.Globalization;
using System.Security.Claims;

namespace CinemaBooking.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Reads the user id from the token; only call it behind [Authorize].</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(TokenClaims.UserId)!, CultureInfo.InvariantCulture);
}