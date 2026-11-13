using System.ComponentModel.DataAnnotations;

namespace StajEvrak.Api.Dtos;

// DTO (Data Transfer Object): API'ye gelen / API'den dönen veri kalıpları.
// Entity sınıflarını (BaseUser vb.) doğrudan dışarı açmayız; böylece
// password_hash gibi alanlar asla yanıtta görünmez.

/// <summary>POST /api/auth/register gövdesi.</summary>
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

/// <summary>POST /api/auth/login gövdesi. Identifier: kullanıcı adı veya e-posta.</summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Kullanıcı bilgisi (şifre hash'i içermez).</summary>
public record UserResponse(int Id, string Username, string Email, string Role, string DashboardView);

/// <summary>Başarılı giriş yanıtı. (Token alanı 4. adımda JWT ile doldurulacak.)</summary>
public record LoginResponse(UserResponse User);

/// <summary>Hata yanıtı.</summary>
public record ErrorResponse(string Message, int? RemainingAttempts = null, DateTime? LockedUntil = null);
