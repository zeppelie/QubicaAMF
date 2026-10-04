using System.Globalization;
using System.Security.Claims;

namespace CinemaBooking.Security;

public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}

public static class TokenClaims
{
    public const string UserId = "sub";
    public const string UserName = "name";
    public const string Role = "role";

    /// <summary>Reads the id of the signed-in user from the token.</summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(UserId)!, CultureInfo.InvariantCulture);
}
