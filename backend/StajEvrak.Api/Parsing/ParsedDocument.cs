namespace StajEvrak.Api.Parsing;

// Belgedeki bir metin parçası ve konumu. X soldan sağa, Y yukarıdan aşağıya artar.
// PDF'te konum sayfadaki noktadır; Word tablosunda sütun ve satır sırasından üretilir.
public sealed record TextCell(string Text, double X, double Y);

// Belgeden okunan ham içerik; oluşturulduktan sonra değiştirilemez.
// TableRows sadece Word'de dolu. Okunamayan belgede IsReadable false olur.
public sealed class ParsedDocument
{
    public bool IsReadable { get; }
    public string? UnreadableReason { get; }
    public IReadOnlyList<string> Lines { get; }
    public IReadOnlyList<IReadOnlyList<string>> TableRows { get; }
    public IReadOnlyList<TextCell> Cells { get; }

    private ParsedDocument(bool isReadable, string? reason, IReadOnlyList<string> lines,
        IReadOnlyList<IReadOnlyList<string>> tableRows, IReadOnlyList<TextCell> cells)
    {
        IsReadable = isReadable;
        UnreadableReason = reason;
        Lines = lines;
        TableRows = tableRows;
        Cells = cells;
    }

    public static ParsedDocument Readable(
        IEnumerable<string> lines, IEnumerable<IReadOnlyList<string>>? tableRows = null,
        IEnumerable<TextCell>? cells = null)
    {
        var cleanLines = lines.Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        var rows = tableRows?.ToList() ?? new List<IReadOnlyList<string>>();
        var cleanCells = cells?.Where(c => c.Text.Trim().Length > 0).ToList() ?? new List<TextCell>();
        return new ParsedDocument(true, null, cleanLines, rows, cleanCells);
    }

    public static ParsedDocument NotReadable(string reason) =>
        new(false, reason, Array.Empty<string>(), Array.Empty<IReadOnlyList<string>>(), Array.Empty<TextCell>());
}
