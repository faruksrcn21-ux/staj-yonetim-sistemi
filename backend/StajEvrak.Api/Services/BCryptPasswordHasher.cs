namespace StajEvrak.Api.Services;

/// <summary>
/// BCrypt ile şifre hash'leme.
/// Hoca: "Şifreler veri tabanında açık şekilde tutulmaz, hash'lenerek saklanır."
/// BCrypt her şifreye rastgele bir "salt" ekler; aynı şifre iki kez hash'lense
/// bile farklı sonuç çıkar. Doğrulama sırasında salt, hash'in içinden okunur.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    // İş faktörü: büyüdükçe hash hesaplamak yavaşlar (kaba kuvvet saldırısı zorlaşır).
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
