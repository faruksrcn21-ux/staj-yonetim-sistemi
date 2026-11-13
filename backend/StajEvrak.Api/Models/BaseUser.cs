namespace StajEvrak.Api.Models;

/// <summary>
/// Tüm kullanıcıların ortak atası (users tablosu).
///
/// NYP:
///  - Soyutlama (Abstraction): Sınıf abstract'tır; doğrudan "kullanıcı" oluşturulamaz,
///    ancak Student veya Admin oluşturulabilir.
///  - Kapsülleme (Encapsulation): Alanların set'i private'tır. Şifre hash'i ve
///    hatalı deneme sayacı dışarıdan değiştirilemez; sadece bu sınıfın
///    metotlarıyla (RegisterFailedAttempt, ResetFailedAttempts) değişir.
///  - Kalıtım (Inheritance): Student ve Admin bu sınıftan türer.
///  - Çok Biçimlilik (Polymorphism): GetDashboardView() her alt sınıfta farklıdır.
/// </summary>
public abstract class BaseUser
{
    /// <summary>Üst üste bu kadar hatalı şifrede hesap kilitlenir (hoca: 5).</summary>
    public const int MaxFailedAttempts = 5;

    /// <summary>Kilit süresi (dokümanda belirtilmemiş, 15 dk seçildi).</summary>
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public int Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>ROLE_STUDENT veya ROLE_ADMIN. Alt sınıf tarafından belirlenir.</summary>
    public string Role { get; private set; } = string.Empty;

    public int FailedAttempts { get; private set; }

    /// <summary>Doluysa ve gelecekteki bir zamansa hesap kilitlidir.</summary>
    public DateTime? LockedUntil { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // EF Core'un veritabanından nesne oluşturabilmesi için gerekli.
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

    /// <summary>Verilen anda hesap kilitli mi?</summary>
    public bool IsLocked(DateTime now) => LockedUntil.HasValue && LockedUntil.Value > now;

    /// <summary>
    /// Hatalı şifre denemesini kaydeder. Sayaç MaxFailedAttempts'a ulaşırsa
    /// hesabı LockDuration kadar kilitler ve sayacı sıfırlar.
    /// </summary>
    /// <returns>Bu deneme sonucunda hesap kilitlendiyse true.</returns>
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

    /// <summary>Başarılı girişte sayaç ve kilit temizlenir.</summary>
    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LockedUntil = null;
    }

    /// <summary>
    /// Girişten sonra kullanıcının yönlendirileceği panel.
    /// Çok biçimlilik: her rol kendi panelini döndürür.
    /// </summary>
    public abstract string GetDashboardView();
}
