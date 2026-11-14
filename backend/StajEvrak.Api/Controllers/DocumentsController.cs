using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documents;

    public DocumentsController(DocumentService documents) => _documents = documents;

    private int CurrentUserId => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [Authorize(Roles = Roles.Student)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentResponse>>> GetMine()
    {
        var list = await _documents.GetByUserAsync(CurrentUserId);
        return Ok(list.Select(DocumentResponse.From));
    }

    [Authorize(Roles = Roles.Student + "," + Roles.Admin)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _documents.CheckAccessAsync(id, CurrentUserId, modify: false);
        return DocumentResults.ToActionResult(this, result);
    }

    [Authorize(Roles = Roles.Student + "," + Roles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _documents.DeleteAsync(id, CurrentUserId);
        return result.Status == DocumentOperationStatus.Success
            ? NoContent()
            : DocumentResults.ToActionResult(this, result);
    }
}

// Buradaki 403'lerde redirectTo yok: kullanıcı giriş yapmış ama bu belgeye izni yok.
internal static class DocumentResults
{
    public static IActionResult ToActionResult(ControllerBase c, DocumentOperationResult r) => r.Status switch
    {
        DocumentOperationStatus.Success => c.Ok(DocumentResponse.From(r.Document!)),
        DocumentOperationStatus.NotFound => c.NotFound(new ErrorResponse(r.Message!)),
        DocumentOperationStatus.NotOwner or DocumentOperationStatus.Completed =>
            c.StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(r.Message!)),
        DocumentOperationStatus.InvalidTransition => c.Conflict(new ErrorResponse(r.Message!)),
        _ => c.StatusCode(StatusCodes.Status500InternalServerError),
    };
}
