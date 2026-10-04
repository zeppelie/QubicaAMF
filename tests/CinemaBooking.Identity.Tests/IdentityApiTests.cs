using System.Net;
using System.Net.Http.Json;
using CinemaBooking.Identity.Api;
using CinemaBooking.Identity.Api.Persistence;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CinemaBooking.Identity.Tests;

[Trait("Category", "Integration")]
public class IdentityApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string Password = "a-long-enough-password";

    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _userName = $"test-{Guid.NewGuid():N}";

    [Fact]
    public async Task A_registered_customer_can_sign_in()
    {
        var registered = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(_userName, Password));
        var user = await registered.Content.ReadFromJsonAsync<UserResponse>();

        var login = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(_userName, Password));
        var token = new JsonWebToken((await login.Content.ReadFromJsonAsync<TokenResponse>())!.AccessToken);

        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.Equal(Roles.Customer, user!.Role);
        Assert.Equal(user.UserId.ToString(), token.Subject);
    }

    [Fact]
    public async Task Refuses_to_register_the_same_user_name_twice()
    {
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(_userName, Password));

        var second = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(_userName, Password));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_password_that_is_too_short()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(_userName, "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refuses_to_sign_in_with_a_wrong_password()
    {
        await _client.PostAsJsonAsync("/auth/register", new RegisterRequest(_userName, Password));

        var login = await _client.PostAsJsonAsync("/auth/login", new LoginRequest(_userName, "not-the-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.UserAccounts.Where(account => account.UserName == _userName).ExecuteDeleteAsync();
    }
}
