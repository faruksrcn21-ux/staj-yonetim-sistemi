namespace StajEvrak.Api.Parsing;

/// Uzantıya göre doğru parser'ı seçer (Çok biçimlilik burada devreye girer).
public class DocumentParserFactory
{
    private readonly IEnumerable<IDocumentParser> _parsers;

    // DI, kayıtlı tüm parser'ları buraya verir.
    public DocumentParserFactory(IEnumerable<IDocumentParser> parsers) => _parsers = parsers;

    public IDocumentParser Select(string extension) =>
        _parsers.FirstOrDefault(p => p.CanParse(extension))
        ?? throw new NotSupportedException($"'{extension}' uzantısı için parser tanımlı değil.");
}