using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace StajEvrak.Api.Parsing;


/// .docx okuyucu. Ek-16 formu tablo tabanlıdır, bu yüzden hem düz satırları hem de tabloları
/// hücre hücre döndürür. Word'deki onay kutuları (sağlık yardım statüsü) işaretliyse ☒, değilse ☐ olarak metinde yer alır; Word kutuyu işaretleyince bu karakteri kendisi günceller.
public class DocxDocumentParser : DocumentParserBase
{
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new[] { ".docx" };

    protected override ParsedDocument ReadContent(string filePath)
    {
        using var word = WordprocessingDocument.Open(filePath, false);
        var body = word.MainDocumentPart?.Document?.Body;
        if (body is null)
            return ParsedDocument.NotReadable("Word belgesinin içeriği bulunamadı.");

        var lines = new List<string>();
        var tableRows = new List<IReadOnlyList<string>>();

        foreach (var element in body.Elements())
        {
            switch (element)
            {
                case Paragraph paragraph:
                    lines.Add(paragraph.InnerText);
                    break;

                case Table table:
                    foreach (var row in table.Elements<TableRow>())
                    {
                        var cells = row.Elements<TableCell>().Select(CellText).ToList();
                        tableRows.Add(cells);
                        foreach (var cell in cells)
                            lines.AddRange(cell.Split('\n'));
                    }
                    break;
            }
        }

        return ParsedDocument.Readable(lines, tableRows);
    }

    private static string CellText(TableCell cell) =>
        string.Join("\n", cell.Elements<Paragraph>()
            .Select(p => p.InnerText.Trim())
            .Where(t => t.Length > 0));
}