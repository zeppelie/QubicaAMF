using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Security;

public static class SecurityExtensions
{
    /// <summary>Makes the service accept the tokens issued by the Identity service.</summary>
    public static IServiceCollection AddJwtSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, settings) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = settings.Value.Issuer,
                    ValidAudience = settings.Value.Audience,
                    IssuerSigningKey = settings.Value.SigningKey,
                    NameClaimType = TokenClaims.UserName,
                    RoleClaimType = TokenClaims.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Describes the bearer token in the OpenAPI document, so the API reference can ask for it.</summary>
    public static OpenApiOptions AddBearerToken(this OpenApiOptions options) =>
        options.AddDocumentTransformer<BearerTokenTransformer>();
}
