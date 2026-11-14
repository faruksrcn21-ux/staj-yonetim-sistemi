namespace StajEvrak.Api.Models;

// Başarısız giriş denemelerinin kaydı.
public class LoginLog
{
    public const string FailedPassword = "FAILED_PASSWORD";
    public const string UnknownUser = "UNKNOWN_USER";
    public const string AccountLocked = "ACCOUNT_LOCKED";
    public const string IpLocked = "IP_LOCKED";

    public int Id { get; private set; }
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
