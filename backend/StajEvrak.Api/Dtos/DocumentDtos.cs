using System.ComponentModel.DataAnnotations;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Dtos;

/// <summary>
/// Belge bilgisi (API yanıtı). Sunucudaki dosya yolu (file_path) güvenlik için dışarı verilmez.
/// </summary>
public record DocumentResponse(
    int Id,
    int UserId,
    string? Username,
    string FileName,
    string FileExtension,
    string Status,
    bool IsReadyForSubmission,
    string? ComparisonReport,
    string? ParsedData,
    string? ManualFormData,
    DateTime UploadedAt)
{
    public static DocumentResponse From(Document d) => new(
        d.Id, d.UserId, d.User?.Username, d.FileName, d.FileExtension, d.Status,
        d.IsReadyForSubmission, d.ComparisonReport, d.ParsedData, d.ManualFormData, d.UploadedAt);
}

/// <summary>Admin'in belgeyi Hatalı/Eksik işaretlerken yazdığı sebep.</summary>
public class MarkDefectiveRequest
{
    [Required(ErrorMessage = "Sebep zorunludur.")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "Sebep en az 3 karakter olmalıdır.")]
    public string Reason { get; set; } = string.Empty;
}
