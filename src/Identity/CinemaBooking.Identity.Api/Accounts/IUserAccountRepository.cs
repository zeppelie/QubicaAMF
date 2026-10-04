using CinemaBooking.Identity.Api.Entities;

namespace CinemaBooking.Identity.Api.Accounts;

public interface IUserAccountRepository
{
    Task<UserAccount?> FindByNameAsync(string userName, CancellationToken cancellationToken);

    /// <summary>Returns false when the user name is already taken.</summary>
    Task<bool> TryAddAsync(UserAccount account, CancellationToken cancellationToken);
}