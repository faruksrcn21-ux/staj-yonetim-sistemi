namespace StajEvrak.Api.Models;

// JWT süresi dolana kadar geçerli kalır; bu yüzden çıkışta token'ın jti'si
// buraya yazılır ve her istekte bu tabloya bakılır.
public class RevokedToken
{
    public int Id { get; private set; }
    public string Jti { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public BaseUser? User { get; private set; }
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
