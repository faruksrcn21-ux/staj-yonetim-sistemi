using System.ComponentModel.DataAnnotations;

namespace StajEvrak.Api.Dtos;

// Entity'ler doğrudan dışarı verilmez; böylece şifre hash'i yanıtta görünmez.

public class RegisterRequest
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-100 karakter olmalıdır.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    // Kullanıcı adı veya e-posta
    [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
}

public record UserResponse(int Id, string Username, string Email, string Role, string DashboardView);

public record LoginResponse(string Token, DateTime ExpiresAt, UserResponse User);

// RedirectTo doluysa frontend kullanıcıyı login sayfasına yönlendirir.
public record ErrorResponse(
    string Message,
    int? RemainingAttempts = null,
    DateTime? LockedUntil = null,
    string? RedirectTo = null);
