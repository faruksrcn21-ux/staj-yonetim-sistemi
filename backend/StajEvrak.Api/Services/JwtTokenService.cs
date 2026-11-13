using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

/// <summary>
/// Giriş başarılı olunca JWT üretir.
/// Hoca: "Giriş sonrası rol bilgisi session veya JWT içinde taşınır."
///
/// Token içindeki bilgiler (claim'ler):
///   sub  : kullanıcı id
///   name : kullanıcı adı
///   role : ROLE_STUDENT / ROLE_ADMIN   (yetki kontrolü buna bakar)
///   jti  : token'ın benzersiz kimliği  (logout'ta iptal etmek için)
///   exp  : bitiş zamanı
/// Token sunucunun gizli anahtarıyla imzalanır; içeriği değiştirilirse imza bozulur
/// ve token reddedilir. Bu yüzden kullanıcı kendi rolünü ADMIN yapamaz.
/// </summary>
public class JwtTokenService
{
    // Claim isimleri (Program.cs'teki doğrulama ayarlarıyla aynı olmalı)
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

    /// <summary>İmzalama ve doğrulamada aynı anahtar kullanılır.</summary>
    public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
