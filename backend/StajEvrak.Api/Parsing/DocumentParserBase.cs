namespace StajEvrak.Api.Parsing;

/// bir kez yazılır. Alt sınıflar sadece "dosyadan içeriği nasıl okurum" kısmını (ReadContent) yazar.
/// Tüm parser'ların ortak davranışı (Kalıtım): dosya var mı kontrolü ve hata yönetimi burada,
/// Bozuk dosya uygulamayı çökertmez, "okunamadı" sonucuna çevrilir.

public abstract class DocumentParserBase : IDocumentParser
{
    protected abstract IReadOnlyCollection<string> SupportedExtensions { get; }

    public bool CanParse(string extension) =>
        SupportedExtensions.Contains(extension.ToLowerInvariant());

    public ParsedDocument Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Okunacak dosya bulunamadı.", filePath);

        try
        {
            return ReadContent(filePath);
        }
        catch (Exception ex)
        {
            return ParsedDocument.NotReadable($"Dosya okunamadı (bozuk olabilir): {ex.Message}");
        }
    }

    protected abstract ParsedDocument ReadContent(string filePath);
}