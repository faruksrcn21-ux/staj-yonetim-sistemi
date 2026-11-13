using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Controllers;

/// <summary>
/// Yetkilendirme katmanını göstermek / test etmek için basit endpoint'ler (hoca Bölüm 4.2).
///   - Token yoksa / geçersizse   -> 403 "giriş yapmanız gerekiyor"  (redirectTo: /login)
///   - Token var ama rol uymuyorsa -> 403 "erişim yetkiniz yok"       (redirectTo: /login)
///   - Rol uyuyorsa               -> 200
///
/// Diğer controller'lar da aynı şekilde [Authorize(Roles = Roles.Student)] veya
/// [Authorize(Roles = Roles.Admin)] yazarak korunur.
/// </summary>
[ApiController]
[Route("api/role-check")]
public class RoleCheckController : ControllerBase
{
    [Authorize(Roles = Roles.Student)]
    [HttpGet("student")]
    public IActionResult StudentOnly() => Ok(new { Message = "Öğrenci ekranına erişim izni var." });

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin")]
    public IActionResult AdminOnly() => Ok(new { Message = "Admin paneline erişim izni var." });
}
