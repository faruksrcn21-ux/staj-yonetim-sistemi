namespace StajEvrak.Api.Parsing;

public class ImageDocumentParser : DocumentParserBase
{
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } =
        new[] { ".jpeg", ".jpg", ".png" };

    protected override ParsedDocument ReadContent(string filePath) =>
        ParsedDocument.NotReadable(
            "Görüntü dosyalarından metin okunamıyor; dosya kabul edildi.");
}

// Eski .doc ikili bir formattır, OpenXML okuyamaz; dosya sadece kabul edilir.
public class LegacyWordDocumentParser : DocumentParserBase
{
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".doc" };

    protected override ParsedDocument ReadContent(string filePath) =>
        ParsedDocument.NotReadable(
            "Eski .doc biçimindeki belgeden metin okunamıyor; belgeyi .docx olarak kaydedip yükleyebilirsiniz.");
}