using StajEvrak.Api.Parsing;

namespace StajEvrak.Api.Dtos;

/// Yükleme isteği (multipart/form-data).
public class UploadDocumentRequest
{
    /// Ek-16 dosyası (.doc, .docx, .pdf, .jpeg, .jpg, .png).
    public IFormFile? File { get; set; }

    ///Öğrencinin formdan elle girdiği bilgiler (JSON metin). Adım B'de tiplenecek.
    public string? ManualFormData { get; set; }
}

///Belgeden ne kadarının okunduğunun özeti.
public record ParseSummary(bool IsReadable, string? Reason, int LineCount, int TableRowCount)
{
    public static ParseSummary From(ParsedDocument parsed) =>
        new(parsed.IsReadable, parsed.UnreadableReason, parsed.Lines.Count, parsed.TableRows.Count);
}

public record UploadDocumentResponse(DocumentResponse Document, ParseSummary Parse);