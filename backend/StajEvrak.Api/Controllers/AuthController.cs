using Microsoft.AspNetCore.Mvc;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Controllers;

/// <summary>
/// Kayıt ve giriş endpoint'leri. İş mantığı AuthService'tedir;
/// controller sadece sonucu uygun HTTP koduna çevirir.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Öğrenci kaydı. 201 Created / 409 Conflict / 400 Bad Request.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request.Username, request.Email, request.Password);

        return result.Status switch
        {
            RegisterStatus.UsernameTaken => Conflict(new ErrorResponse("Bu kullanıcı adı zaten kullanılıyor.")),
            RegisterStatus.EmailTaken => Conflict(new ErrorResponse("Bu e-posta zaten kayıtlı.")),
            _ => StatusCode(StatusCodes.Status201Created, ToUserResponse(result.Student!)),
        };
    }

    /// <summary>
    /// Giriş. 200 OK / 401 Unauthorized (yanlış bilgi) /
    /// 423 Locked (hesap kilitli) / 429 Too Many Requests (IP kilitli).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.LoginAsync(request.Identifier, request.Password, ip);

        return result.Status switch
        {
            LoginStatus.Success => Ok(new LoginResponse(ToUserResponse(result.User!))),

            // Güvenlik: kullanıcı adının mı şifrenin mi yanlış olduğu söylenmez.
            LoginStatus.InvalidCredentials => Unauthorized(new ErrorResponse(
                "Kullanıcı adı/e-posta veya şifre hatalı.",
                RemainingAttempts: result.RemainingAttempts)),

            LoginStatus.AccountLocked => StatusCode(StatusCodes.Status423Locked, new ErrorResponse(
                "5 hatalı deneme nedeniyle hesabınız geçici olarak kilitlendi.",
                LockedUntil: result.LockedUntil)),

            LoginStatus.IpLocked => StatusCode(StatusCodes.Status429TooManyRequests, new ErrorResponse(
                "Çok fazla hatalı giriş denemesi. Lütfen daha sonra tekrar deneyin.",
                LockedUntil: result.LockedUntil)),

            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    private static UserResponse ToUserResponse(BaseUser user) =>
        new(user.Id, user.Username, user.Email, user.Role, user.GetDashboardView());
}
