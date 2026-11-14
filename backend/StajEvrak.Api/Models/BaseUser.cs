namespace StajEvrak.Api.Models;

// Soyutlama: abstract sınıf, doğrudan nesnesi oluşturulamaz.
// Kapsülleme: alanlar private set; sadece bu sınıfın metotlarıyla değişir.
// Kalıtım: Student ve Admin bu sınıftan türer.
// Çok biçimlilik: GetDashboardView ve CheckDocumentAccess her rolde farklı çalışır.
public abstract class BaseUser
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public int FailedAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected BaseUser() { }

    protected BaseUser(string username, string email, string passwordHash, string role)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        FailedAttempts = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsLocked(DateTime now) => LockedUntil.HasValue && LockedUntil.Value > now;

    // 5. hatalı denemede hesap kilitlenir; kilitlendiyse true döner.
    public bool RegisterFailedAttempt(DateTime now)
    {
        FailedAttempts++;

        if (FailedAttempts >= MaxFailedAttempts)
        {
            LockedUntil = now.Add(LockDuration);
            FailedAttempts = 0;
            return true;
        }

        return false;
    }

    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LockedUntil = null;
    }

    public abstract string GetDashboardView();

    // modify: true ise değiştirme/silme, false ise sadece görüntüleme.
    public abstract DocumentAccess CheckDocumentAccess(Document document, bool modify);
}
