namespace StajEvrak.Api.Parsing;

/// JPEG/PNG: Resimden metin okumak

public class ImageDocumentParser : DocumentParserBase
{
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } =
        new[] { ".jpeg", ".jpg", ".png" };

    protected override ParsedDocument ReadContent(string filePath) =>
        ParsedDocument.NotReadable(
            "Görüntü dosyalarından metin okuma (OCR) final aşamasındadır; dosya sadece kabul edildi.");
}
/// Eski .doc biçimi ikili bir formattır, OpenXML kütüphanesi onu okuyamaz. Dosya kabul edilir


public class LegacyWordDocumentParser : DocumentParserBase
{
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".doc" };

    protected override ParsedDocument ReadContent(string filePath) =>
        ParsedDocument.NotReadable(
            "Eski .doc biçimi vizede okunmuyor; belgeyi .docx olarak kaydedip yükleyebilirsiniz.");
}