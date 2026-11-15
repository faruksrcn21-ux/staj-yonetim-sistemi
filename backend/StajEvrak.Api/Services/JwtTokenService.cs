using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

// Token gizli anahtarla imzalanır; içindeki rol değiştirilirse imza bozulur ve token reddedilir.
public class JwtTokenService
{
    public const string RoleClaim = "role";
    public const string NameClaim = JwtRegisteredClaimNames.Name;

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(BaseUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(NameClaim, user.Username),
                new Claim(RoleClaim, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            }),
            SigningCredentials = new SigningCredentials(
                CreateSigningKey(_options.Key), SecurityAlgorithms.HmacSha256),
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expiresAt);
    }

    public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
