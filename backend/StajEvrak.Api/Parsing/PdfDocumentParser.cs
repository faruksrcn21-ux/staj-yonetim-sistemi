using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace StajEvrak.Api.Parsing;

/// PDF okuyucu (PdfPig). PDF'te tablo yapısı yoktur, sadece konumlu kelimeler vardır. Aynı hizadaki kelimeleri tek satırda birleştiriyoruz. Not: iki sütunlu formlarda sol ve sağ hücrenin metni aynı satıra düşer; bunu alan ayıklama aşaması (Adım B) çözecek.

public class PdfDocumentParser : DocumentParserBase
{
    // Aynı satırdaki kelimelerin dikey konum farkı (PDF birimi) bu değerden küçükse aynı satırdır.
    private const double LineTolerance = 3.0;

    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".pdf" };

    protected override ParsedDocument ReadContent(string filePath)
    {
        using var pdf = PdfDocument.Open(filePath);

        var lines = new List<string>();
        foreach (var page in pdf.GetPages())
            lines.AddRange(ReadLines(page));

        if (lines.Count == 0)
            return ParsedDocument.NotReadable(
                "PDF'te okunabilir metin bulunamadı (taranmış görüntü olabilir; OCR final aşamasındadır).");

        return ParsedDocument.Readable(lines);
    }

    private static IEnumerable<string> ReadLines(Page page)
    {
        var rows = new List<List<Word>>();

        // Yukarıdan aşağıya, soldan sağa
        var words = page.GetWords()
            .OrderByDescending(w => w.BoundingBox.Bottom)
            .ThenBy(w => w.BoundingBox.Left);

        foreach (var word in words)
        {
            var row = rows.FirstOrDefault(r =>
                Math.Abs(r[0].BoundingBox.Bottom - word.BoundingBox.Bottom) <= LineTolerance);

            if (row is null)
                rows.Add(new List<Word> { word });
            else
                row.Add(word);
        }

        return rows.Select(r => string.Join(" ", r.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));
    }
}