namespace StajEvrak.Api.Services;

/// <summary>appsettings.json içindeki "Jwt" bölümü.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;   // Token'ı kim üretti
    public string Audience { get; set; } = string.Empty; // Token kimin için
    public string Key { get; set; } = string.Empty;      // İmza anahtarı (gizli, en az 32 karakter)
    public int ExpiresMinutes { get; set; } = 60;        // Token geçerlilik süresi
}
