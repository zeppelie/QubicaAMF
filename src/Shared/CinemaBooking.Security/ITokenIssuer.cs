namespace CinemaBooking.Security;

/// <summary>Creates the access tokens accepted by every service.</summary>
public interface ITokenIssuer
{
    /// <summary>Issues a signed token that carries the id, name and role of the user.</summary>
    AccessToken Issue(int userId, string userName, string role);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
