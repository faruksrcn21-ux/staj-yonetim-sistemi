namespace StajEvrak.Api.Services;

// Soyutlama: AuthService hangi hash algoritmasının kullanıldığını bilmez.
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
