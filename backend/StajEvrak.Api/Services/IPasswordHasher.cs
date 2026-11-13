namespace StajEvrak.Api.Services;

/// <summary>
/// Şifre hash'leme sözleşmesi (Soyutlama).
/// AuthService hangi algoritmanın kullanıldığını bilmez; sadece bu arayüzü kullanır.
/// İleride BCrypt yerine başka bir algoritma gerekirse sadece yeni bir sınıf yazılır.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
