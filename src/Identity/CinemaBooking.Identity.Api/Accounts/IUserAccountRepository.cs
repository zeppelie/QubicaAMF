using CinemaBooking.Identity.Api.Entities;

namespace CinemaBooking.Identity.Api.Accounts;

/// <summary>Reads and stores user accounts.</summary>
public interface IUserAccountRepository
{
    /// <summary>Returns the account with the given user name, or null when it is missing.</summary>
    Task<UserAccount?> FindByNameAsync(string userName, CancellationToken cancellationToken);

    /// <summary>Stores a new account, or returns false when the user name is already taken.</summary>
    Task<bool> TryAddAsync(UserAccount account, CancellationToken cancellationToken);
}
