using CinemaBooking.Identity.Api.Entities;
using CinemaBooking.Security;

namespace CinemaBooking.Identity.Api.Accounts;

public interface IAccountService
{
    /// <summary>Creates a customer, or returns null when the user name is already taken.</summary>
    Task<UserAccount?> RegisterAsync(string userName, string password, CancellationToken cancellationToken);

    /// <summary>Returns null when user name and password do not match.</summary>
    Task<AccessToken?> LoginAsync(string userName, string password, CancellationToken cancellationToken);
}