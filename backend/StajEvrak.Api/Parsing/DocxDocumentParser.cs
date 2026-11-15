using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace StajEvrak.Api.Parsing;

// Ek-16 tablo tabanlı olduğu için hem satırlar hem tablo hücreleri okunur.
// İşaretli onay kutusu metinde ☒, boş olan ☐ olarak gelir.
// Hücrelere PDF'tekine benzer konum verilir: X = sütun sırası, Y = satır sırası.
public class DocxDocumentParser : DocumentParserBase
{
    private const double ColumnStep = 100;
    private const double RowStep = 100;

    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".docx" };

    protected override ParsedDocument ReadContent(string filePath)
    {
        using var word = WordprocessingDocument.Open(filePath, false);
        var body = word.MainDocumentPart?.Document?.Body;
        if (body is null)
            return ParsedDocument.NotReadable("Word belgesinin içeriği bulunamadı.");

        var lines = new List<string>();
        var tableRows = new List<IReadOnlyList<string>>();
        var cells = new List<TextCell>();
        var rowIndex = 0;

        foreach (var element in body.Elements())
        {
            switch (element)
            {
                case Paragraph paragraph:
                    lines.Add(paragraph.InnerText);
                    cells.Add(new TextCell(paragraph.InnerText, 0, rowIndex++ * RowStep));
                    break;

                case Table table:
                    foreach (var row in table.Elements<TableRow>())
                    {
                        var texts = new List<string>();
                        var column = 0;
                        foreach (var cell in row.Elements<TableCell>())
                        {
                            var text = CellText(cell);
                            texts.Add(text);
                            lines.AddRange(text.Split('\n'));
                            cells.Add(new TextCell(text.Replace('\n', ' '), column * ColumnStep, rowIndex * RowStep));

                            // Birleştirilmiş hücre birden fazla sütun kaplar.
                            column += cell.TableCellProperties?.GridSpan?.Val?.Value ?? 1;
                        }
                        tableRows.Add(texts);
                        rowIndex++;
                    }
                    break;
            }
        }

        return ParsedDocument.Readable(lines, tableRows, cells);
    }

    private static string CellText(TableCell cell) =>
        string.Join("\n", cell.Elements<Paragraph>()
            .Select(p => p.InnerText.Trim())
            .Where(t => t.Length > 0));
}
