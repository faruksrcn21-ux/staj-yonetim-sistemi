namespace StajEvrak.Api.Models;

/// <summary>
/// Başarısız giriş denemesi kaydı (login_logs tablosu).
/// Hoca: "Başarısız giriş denemeleri loglanır."
/// </summary>
public class LoginLog
{
    // log_status değerleri
    public const string FailedPassword = "FAILED_PASSWORD"; // kullanıcı var, şifre yanlış
    public const string UnknownUser = "UNKNOWN_USER";       // böyle bir kullanıcı adı/e-posta yok
    public const string AccountLocked = "ACCOUNT_LOCKED";   // hesap kilitliyken giriş denendi
    public const string IpLocked = "IP_LOCKED";             // IP kilitliyken giriş denendi

    public int Id { get; private set; }

    /// <summary>Var olmayan kullanıcı adıyla denemelerde null'dır.</summary>
    public int? UserId { get; private set; }
    public BaseUser? User { get; private set; }

    public string AttemptedIdentifier { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public DateTime AttemptTime { get; private set; }
    public string LogStatus { get; private set; } = string.Empty;

    private LoginLog() { }

    public LoginLog(int? userId, string attemptedIdentifier, string ipAddress, string logStatus, DateTime attemptTime)
    {
        UserId = userId;
        AttemptedIdentifier = attemptedIdentifier;
        IpAddress = ipAddress;
        LogStatus = logStatus;
        AttemptTime = attemptTime;
    }
}
