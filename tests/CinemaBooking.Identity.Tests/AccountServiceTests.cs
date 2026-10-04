using CinemaBooking.Identity.Api.Accounts;
using CinemaBooking.Identity.Api.Entities;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CinemaBooking.Identity.Tests;

public class AccountServiceTests
{
    private const string Password = "a-long-enough-password";

    private readonly InMemoryAccounts _accounts = new();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        var settings = Options.Create(new JwtSettings
        {
            Issuer = "tests",
            Audience = "tests",
            Key = "a-signing-key-that-is-only-used-by-tests"
        });
        _service = new AccountService(
            _accounts, new PasswordHasher<UserAccount>(), new JwtTokenIssuer(settings, TimeProvider.System));
    }

    [Fact]
    public async Task Registers_a_customer_without_storing_the_password()
    {
        var account = await _service.RegisterAsync(" alice ", Password, CancellationToken.None);

        Assert.Equal("alice", account!.UserName);
        Assert.Equal(Roles.Customer, account.Role);
        Assert.DoesNotContain(Password, account.PasswordHash);
    }

    [Fact]
    public async Task Refuses_a_user_name_that_is_already_taken()
    {
        await _service.RegisterAsync("alice", Password, CancellationToken.None);

        var second = await _service.RegisterAsync("alice", "another-password", CancellationToken.None);

        Assert.Null(second);
    }

    [Fact]
    public async Task Signs_in_with_the_right_password_and_puts_the_user_in_the_token()
    {
        var account = await _service.RegisterAsync("alice", Password, CancellationToken.None);

        var token = await _service.LoginAsync("alice", Password, CancellationToken.None);

        var claims = new JsonWebToken(token!.Value);
        Assert.Equal(account!.UserId.ToString(), claims.Subject);
        Assert.Equal(Roles.Customer, claims.GetClaim(TokenClaims.Role).Value);
        Assert.True(token.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Does_not_sign_in_with_a_wrong_password()
    {
        await _service.RegisterAsync("alice", Password, CancellationToken.None);

        Assert.Null(await _service.LoginAsync("alice", "not-the-password", CancellationToken.None));
    }

    [Fact]
    public async Task Does_not_sign_in_an_unknown_user()
    {
        Assert.Null(await _service.LoginAsync("nobody", Password, CancellationToken.None));
    }

    private sealed class InMemoryAccounts : IUserAccountRepository
    {
        private readonly List<UserAccount> _accounts = [];

        public Task<UserAccount?> FindByNameAsync(string userName, CancellationToken cancellationToken) =>
            Task.FromResult(_accounts.FirstOrDefault(account => account.UserName == userName));

        public Task<bool> TryAddAsync(UserAccount account, CancellationToken cancellationToken)
        {
            if (_accounts.Any(existing => existing.UserName == account.UserName))
                return Task.FromResult(false);

            account.UserId = _accounts.Count + 1;
            _accounts.Add(account);
            return Task.FromResult(true);
        }
    }
}
