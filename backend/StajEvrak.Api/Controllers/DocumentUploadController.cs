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
public class DocumentUploadController : ControllerBase
{
    private readonly DocumentUploadService _uploads;

    public DocumentUploadController(DocumentUploadService uploads) => _uploads = uploads;

    private int CurrentUserId => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [Authorize(Roles = Roles.Student)]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new ErrorResponse("Lütfen bir dosya seçin."));

        await using var stream = request.File.OpenReadStream();
        var result = await _uploads.UploadAsync(
            CurrentUserId, request.File.FileName, request.File.Length, stream, request.ManualFormData);

        if (result.InvalidFileMessage is not null)
            return BadRequest(new ErrorResponse(result.InvalidFileMessage));

        var response = new UploadDocumentResponse(DocumentResponse.From(result.Document!), result.Parse!);
        return Created($"/api/documents/{result.Document!.Id}", response);
    }

    [Authorize(Roles = Roles.Student + "," + Roles.Admin)]
    [HttpPost("{id:int}/reupload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Reupload(int id, [FromForm] UploadDocumentRequest request)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new ErrorResponse("Lütfen bir dosya seçin."));

        await using var stream = request.File.OpenReadStream();
        var result = await _uploads.ReuploadAsync(
            id, CurrentUserId, request.File.FileName, request.File.Length, stream, request.ManualFormData);

        
        if (result.AccessDenied is not null)
            return DocumentResults.ToActionResult(this, result.AccessDenied);

        if (result.InvalidFileMessage is not null)
            return BadRequest(new ErrorResponse(result.InvalidFileMessage));

        return Ok(new UploadDocumentResponse(DocumentResponse.From(result.Document!), result.Parse!));
    }
}