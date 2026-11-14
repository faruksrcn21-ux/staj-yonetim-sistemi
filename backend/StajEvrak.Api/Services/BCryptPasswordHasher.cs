namespace StajEvrak.Api.Services;

// BCrypt her şifreye rastgele salt ekler; aynı şifrenin hash'i bile her seferinde farklı olur.
public class BCryptPasswordHasher : IPasswordHasher
{
    // Büyüdükçe hash yavaşlar, kaba kuvvet saldırısı zorlaşır.
    private const int WorkFactor = 11;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash) => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
