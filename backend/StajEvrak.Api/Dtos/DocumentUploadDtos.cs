using StajEvrak.Api.Parsing;

namespace StajEvrak.Api.Dtos;

public class UploadDocumentRequest
{
    public IFormFile? File { get; set; }

    // Öğrencinin formdan elle girdiği bilgiler (JSON)
    public string? ManualFormData { get; set; }
}

public record ParseSummary(bool IsReadable, string? Reason, int LineCount, int TableRowCount)
{
    public static ParseSummary From(ParsedDocument parsed) =>
        new(parsed.IsReadable, parsed.UnreadableReason, parsed.Lines.Count, parsed.TableRows.Count);
}

public record UploadDocumentResponse(DocumentResponse Document, ParseSummary Parse);