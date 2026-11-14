using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

public enum RegisterStatus { Success, UsernameTaken, EmailTaken }

public record RegisterResult(RegisterStatus Status, Student? Student = null);

public enum LoginStatus { Success, InvalidCredentials, AccountLocked, IpLocked }

public record LoginResult(
    LoginStatus Status,
    BaseUser? User = null,
    int? RemainingAttempts = null,
    DateTime? LockedUntil = null);

public class AuthService
{
    public static readonly TimeSpan IpWindow = TimeSpan.FromMinutes(15);
    public const int MaxFailedAttemptsPerIp = 5;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(AppDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // Kayıt ekranından sadece öğrenci oluşturulur.
    public async Task<RegisterResult> RegisterAsync(string username, string email, string password)
    {
        username = username.Trim();
        email = NormalizeEmail(email);

        if (await _db.Users.AnyAsync(u => u.Username == username))
            return new RegisterResult(RegisterStatus.UsernameTaken);

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return new RegisterResult(RegisterStatus.EmailTaken);

        // Şifre asla açık saklanmaz; sadece hash'i kaydedilir.
        var student = new Student(username, email, _hasher.Hash(password));

        _db.Students.Add(student);
        await _db.SaveChangesAsync();

        return new RegisterResult(RegisterStatus.Success, student);
    }

    public async Task<LoginResult> LoginAsync(string identifier, string password, string ipAddress)
    {
        var now = DateTime.UtcNow;
        identifier = identifier.Trim();

        // 1) IP kilidi
        var ipLockedUntil = await GetIpLockedUntilAsync(ipAddress, now);
        if (ipLockedUntil is not null)
        {
            await LogAsync(null, identifier, ipAddress, LoginLog.IpLocked, now);
            return new LoginResult(LoginStatus.IpLocked, LockedUntil: ipLockedUntil);
        }

        // 2) Kullanıcı
        var user = await FindByIdentifierAsync(identifier);
        if (user is null)
        {
            await LogAsync(null, identifier, ipAddress, LoginLog.UnknownUser, now);

            // Bu deneme 5. yanlış kullanıcı adıysa IP şimdi kilitlenmiştir.
            var lockedUntilNow = await GetIpLockedUntilAsync(ipAddress, now);
            return lockedUntilNow is not null
                ? new LoginResult(LoginStatus.IpLocked, LockedUntil: lockedUntilNow)
                : new LoginResult(LoginStatus.InvalidCredentials);
        }

        // 3) Hesap kilidi
        if (user.IsLocked(now))
        {
            await LogAsync(user.Id, identifier, ipAddress, LoginLog.AccountLocked, now);
            return new LoginResult(LoginStatus.AccountLocked, LockedUntil: user.LockedUntil);
        }

        // 4) Şifre kontrolü
        if (!_hasher.Verify(password, user.PasswordHash))
        {
            bool lockedNow = user.RegisterFailedAttempt(now);
            await LogAsync(user.Id, identifier, ipAddress, LoginLog.FailedPassword, now);

            return lockedNow
                ? new LoginResult(LoginStatus.AccountLocked, LockedUntil: user.LockedUntil)
                : new LoginResult(LoginStatus.InvalidCredentials,
                    RemainingAttempts: BaseUser.MaxFailedAttempts - user.FailedAttempts);
        }

        // 5) Başarılı giriş
        user.ResetFailedAttempts();
        await _db.SaveChangesAsync();

        return new LoginResult(LoginStatus.Success, user);
    }

    // Token'ın jti'si iptal listesine yazılır; her istekte bu listeye bakılır.
    public async Task LogoutAsync(string jti, int userId, DateTime tokenExpiresAt)
    {
        var now = DateTime.UtcNow;

        if (!await _db.RevokedTokens.AnyAsync(t => t.Jti == jti))
            _db.RevokedTokens.Add(new RevokedToken(jti, userId, tokenExpiresAt, now));

        // Süresi dolmuş token'lar zaten geçersiz; kayıtları temizlenir.
        var expired = await _db.RevokedTokens.Where(t => t.ExpiresAt < now).ToListAsync();
        _db.RevokedTokens.RemoveRange(expired);

        await _db.SaveChangesAsync();
    }

    public Task<bool> IsTokenRevokedAsync(string jti) =>
        _db.RevokedTokens.AnyAsync(t => t.Jti == jti);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private Task<BaseUser?> FindByIdentifierAsync(string identifier)
    {
        if (identifier.Contains('@'))
        {
            var email = NormalizeEmail(identifier);
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        return _db.Users.FirstOrDefaultAsync(u => u.Username == identifier);
    }

    // Yanlış şifrede hesap kilitlenir. Yanlış kullanıcı adında kilitlenecek hesap olmadığı için
    // IP sayılır: son 15 dakikada bu IP'den 5 deneme varsa IP kilitlidir.
    private async Task<DateTime?> GetIpLockedUntilAsync(string ipAddress, DateTime now)
    {
        var windowStart = now - IpWindow;

        var recentFailures = await _db.LoginLogs
            .Where(l => l.IpAddress == ipAddress
                        && l.AttemptTime > windowStart
                        && l.LogStatus == LoginLog.UnknownUser)
            .OrderByDescending(l => l.AttemptTime)
            .Select(l => l.AttemptTime)
            .Take(MaxFailedAttemptsPerIp)
            .ToListAsync();

        if (recentFailures.Count < MaxFailedAttemptsPerIp)
            return null;

        // Son 5 denemenin en eskisi pencereden çıkınca kilit kalkar.
        return recentFailures[^1] + IpWindow;
    }

    private async Task LogAsync(int? userId, string identifier, string ipAddress, string status, DateTime now)
    {
        _db.LoginLogs.Add(new LoginLog(userId, identifier, ipAddress, status, now));
        await _db.SaveChangesAsync();
    }
}
