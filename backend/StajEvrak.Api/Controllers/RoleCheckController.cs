using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Controllers;

// Rol kontrolünü test etmek için: rol uymazsa 403 döner.
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
