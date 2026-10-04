using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CinemaBooking.Security;

public sealed class JwtTokenIssuer(IOptions<JwtSettings> settings, TimeProvider clock) : ITokenIssuer
{
    public AccessToken Issue(int userId, string userName, string role)
    {
        var jwt = settings.Value;
        var now = clock.GetUtcNow();
        var expiresAt = now.AddMinutes(jwt.LifetimeMinutes);

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(TokenClaims.UserId, userId.ToString(CultureInfo.InvariantCulture)),
                new Claim(TokenClaims.UserName, userName),
                new Claim(TokenClaims.Role, role)
            ]),
            SigningCredentials = new SigningCredentials(jwt.SigningKey, SecurityAlgorithms.HmacSha256)
        });

        return new AccessToken(token, expiresAt);
    }
}
