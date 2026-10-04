namespace CinemaBooking.Security;

public interface ITokenIssuer
{
    /// <summary>Signs a token carrying the id, name and role of the user.</summary>
    AccessToken Issue(int userId, string userName, string role);
}