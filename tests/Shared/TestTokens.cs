using System.Net.Http.Headers;
using CinemaBooking.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CinemaBooking.Tests.Shared;

internal static class TestTokens
{
    public static HttpClient CreateClientFor<TProgram>(this WebApplicationFactory<TProgram> factory, int userId, string role)
        where TProgram : class
    {
        var settings = factory.Services.GetRequiredService<IOptions<JwtSettings>>();
        var token = new JwtTokenIssuer(settings, TimeProvider.System).Issue(userId, $"user-{userId}", role);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        return client;
    }
}
