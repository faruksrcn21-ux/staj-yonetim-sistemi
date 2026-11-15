using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace StajEvrak.Api.Parsing;

// PDF'te tablo yoktur, sadece konumlu kelimeler vardır; aynı hizadakiler tek satırda birleştirilir.
// Satır içinde büyük boşlukla ayrılan kelime grupları ayrı hücre sayılır (tablonun sütunları).
public class PdfDocumentParser : DocumentParserBase
{
    // Dikey konum farkı bundan küçük olan kelimeler aynı satır sayılır.
    private const double LineTolerance = 3.0;

    // Yatay boşluk bundan büyükse kelimeler farklı hücrededir.
    private const double CellGap = 8.0;

    // Sayfaların Y değerleri karışmasın diye her sayfa bu kadar aşağı kaydırılır.
    private const double PageOffset = 10000;

    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".pdf" };

    protected override ParsedDocument ReadContent(string filePath)
    {
        using var pdf = PdfDocument.Open(filePath);

        var lines = new List<string>();
        var cells = new List<TextCell>();
        var pageIndex = 0;
        foreach (var page in pdf.GetPages())
        {
            foreach (var row in ReadRows(page))
            {
                lines.Add(string.Join(" ", row.Select(w => w.Text)));
                cells.AddRange(SplitCells(row, page.Height - row[0].BoundingBox.Bottom + pageIndex * PageOffset));
            }
            pageIndex++;
        }

        if (lines.Count == 0)
            return ParsedDocument.NotReadable(
                "PDF'te okunabilir metin bulunamadı; belge taranmış bir görüntü olabilir.");

        return ParsedDocument.Readable(lines, cells: cells);
    }

    private static List<List<Word>> ReadRows(Page page)
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

        return rows.Select(r => r.OrderBy(w => w.BoundingBox.Left).ToList()).ToList();
    }

    private static IEnumerable<TextCell> SplitCells(List<Word> row, double y)
    {
        var start = 0;
        for (var i = 1; i <= row.Count; i++)
        {
            if (i < row.Count && row[i].BoundingBox.Left - row[i - 1].BoundingBox.Right <= CellGap)
                continue;

            var text = string.Join(" ", row.Skip(start).Take(i - start).Select(w => w.Text));
            yield return new TextCell(text, row[start].BoundingBox.Left, y);
            start = i;
        }
    }
}
