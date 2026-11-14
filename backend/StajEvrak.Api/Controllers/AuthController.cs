using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtTokenService _tokens;

    public AuthController(AuthService auth, JwtTokenService tokens)
    {
        _auth = auth;
        _tokens = tokens;
    }

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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _auth.LoginAsync(request.Identifier, request.Password, ip);

        return result.Status switch
        {
            LoginStatus.Success => Ok(CreateLoginResponse(result.User!)),

            // Kullanıcı adının mı şifrenin mi yanlış olduğu söylenmez.
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

    // Token iptal edilir; aynı token ile gelen sonraki istekler 403 alır.
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti)!;
        var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var exp = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Exp)!);
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;

        await _auth.LogoutAsync(jti, userId, expiresAt);

        return Ok(new { Message = "Çıkış yapıldı.", RedirectTo = "/login" });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new
    {
        Id = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
        Username = User.FindFirstValue(JwtTokenService.NameClaim),
        Role = User.FindFirstValue(JwtTokenService.RoleClaim),
    });

    private LoginResponse CreateLoginResponse(BaseUser user)
    {
        var (token, expiresAt) = _tokens.CreateToken(user);
        return new LoginResponse(token, expiresAt, ToUserResponse(user));
    }

    private static UserResponse ToUserResponse(BaseUser user) =>
        new(user.Id, user.Username, user.Email, user.Role, user.GetDashboardView());
}
