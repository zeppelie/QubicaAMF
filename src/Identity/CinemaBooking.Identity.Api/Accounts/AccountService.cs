using CinemaBooking.Identity.Api.Entities;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Identity;

namespace CinemaBooking.Identity.Api.Accounts;

public sealed class AccountService(
    IUserAccountRepository repository,
    IPasswordHasher<UserAccount> passwordHasher,
    ITokenIssuer tokenIssuer) : IAccountService
{
    public async Task<UserAccount?> RegisterAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var account = new UserAccount { UserName = userName.Trim(), Role = Roles.Customer };
        account.PasswordHash = passwordHasher.HashPassword(account, password);

        return await repository.TryAddAsync(account, cancellationToken) ? account : null;
    }

    public async Task<AccessToken?> LoginAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var account = await repository.FindByNameAsync(userName.Trim(), cancellationToken);
        if (account is null)
            return null;

        var check = passwordHasher.VerifyHashedPassword(account, account.PasswordHash, password);
        return check == PasswordVerificationResult.Failed
            ? null
            : tokenIssuer.Issue(account.UserId, account.UserName, account.Role);
    }
}
