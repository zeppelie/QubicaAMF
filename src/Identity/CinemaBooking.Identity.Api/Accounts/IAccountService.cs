using CinemaBooking.Identity.Api.Entities;
using CinemaBooking.Security;

namespace CinemaBooking.Identity.Api.Accounts;

/// <summary>Registers customers and signs users in.</summary>
public interface IAccountService
{
    /// <summary>Creates a customer account, or returns null when the user name is already taken.</summary>
    Task<UserAccount?> RegisterAsync(string userName, string password, CancellationToken cancellationToken);

    /// <summary>Returns an access token when user name and password match, otherwise null.</summary>
    Task<AccessToken?> LoginAsync(string userName, string password, CancellationToken cancellationToken);
}
