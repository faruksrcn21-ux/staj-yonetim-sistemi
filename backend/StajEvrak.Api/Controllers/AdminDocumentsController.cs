using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Controllers;

/// <summary>
/// Admin belge işlemleri (sadece ROLE_ADMIN; diğerleri 403).
///
///   GET  /api/admin/documents?status=TAMAMLANDI  → tüm belgeler, istenirse statüye göre
///                                                  (hoca 4.2/4.7: "Tamamlandı" ayrı sekmede)
///   POST /api/admin/documents/{id}/review        → Yüklendi → İnceleniyor
///   POST /api/admin/documents/{id}/approve       → İnceleniyor → Onaylandı → Tamamlandı
///   POST /api/admin/documents/{id}/mark-defective {reason} → İnceleniyor → Hatalı/Eksik
///
/// Akışa aykırı istek (ör. Hatalı/Eksik belgeyi onaylamak) → 409.
/// </summary>
[ApiController]
[Route("api/admin/documents")]
[Authorize(Roles = Roles.Admin)]
public class AdminDocumentsController : ControllerBase
{
    private static readonly string[] ValidStatuses =
    [
        DocumentStatus.Yuklendi, DocumentStatus.Inceleniyor, DocumentStatus.HataliEksik,
        DocumentStatus.Onaylandi, DocumentStatus.Tamamlandi,
    ];

    private readonly DocumentService _documents;

    public AdminDocumentsController(DocumentService documents) => _documents = documents;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (!string.IsNullOrEmpty(status) && !ValidStatuses.Contains(status))
            return BadRequest(new ErrorResponse($"Geçersiz statü. Geçerli değerler: {string.Join(", ", ValidStatuses)}"));

        var list = await _documents.GetAllAsync(status);
        return Ok(list.Select(DocumentResponse.From));
    }

    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> StartReview(int id) =>
        DocumentResults.ToActionResult(this, await _documents.StartReviewAsync(id));

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id) =>
        DocumentResults.ToActionResult(this, await _documents.ApproveAsync(id));

    [HttpPost("{id:int}/mark-defective")]
    public async Task<IActionResult> MarkDefective(int id, [FromBody] MarkDefectiveRequest request) =>
        DocumentResults.ToActionResult(this, await _documents.MarkDefectiveAsync(id, request.Reason));
}
