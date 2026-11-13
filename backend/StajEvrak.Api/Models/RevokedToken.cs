namespace StajEvrak.Api.Models;

/// <summary>
/// Çıkış (logout) yapılmış JWT'ler (revoked_tokens tablosu).
/// Hoca: "Çıkış işlemi token/session geçersiz kılar."
/// JWT kendi başına süresi dolana kadar geçerli olduğu için, çıkışta token'ın
/// kimliği (jti) buraya yazılır ve her istekte bu tabloya bakılır.
/// </summary>
public class RevokedToken
{
    public int Id { get; private set; }

    /// <summary>JWT'nin benzersiz kimliği ("jti" claim'i).</summary>
    public string Jti { get; private set; } = string.Empty;

    public int UserId { get; private set; }
    public BaseUser? User { get; private set; }

    /// <summary>Token'ın asıl bitiş zamanı; bu zamandan sonra satır silinebilir.</summary>
    public DateTime ExpiresAt { get; private set; }

    public DateTime RevokedAt { get; private set; }

    private RevokedToken() { }

    public RevokedToken(string jti, int userId, DateTime expiresAt, DateTime revokedAt)
    {
        Jti = jti;
        UserId = userId;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }
}
