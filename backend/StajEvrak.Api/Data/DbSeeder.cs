using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Data;

/// <summary>appsettings içindeki "AdminSeed" bölümü.</summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Uygulama açılışında başlangıç verilerini oluşturur.
/// Kayıt ekranı sadece öğrenci oluşturduğu için admin hesabı burada oluşturulur.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAdminAsync(
        AppDbContext db, IPasswordHasher hasher, AdminSeedOptions options, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(options.Username)
            || string.IsNullOrWhiteSpace(options.Email)
            || string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning("AdminSeed ayarları eksik; admin hesabı oluşturulmadı.");
            return;
        }

        // Zaten bir admin varsa hiçbir şey yapma (her açılışta tekrar oluşturmaz).
        if (await db.Admins.AnyAsync())
            return;

        var username = options.Username.Trim();
        var email = options.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Username == username || u.Email == email))
        {
            logger.LogWarning(
                "AdminSeed: '{Username}' kullanıcı adı veya '{Email}' e-postası başka bir hesapta kullanılıyor; admin oluşturulmadı.",
                username, email);
            return;
        }

        db.Admins.Add(new Admin(username, email, hasher.Hash(options.Password)));
        await db.SaveChangesAsync();

        logger.LogInformation("Admin hesabı oluşturuldu: {Username}", username);
    }
}
