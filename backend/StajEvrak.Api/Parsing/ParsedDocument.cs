namespace StajEvrak.Api.Parsing;

// Belgeden okunan ham içerik; oluşturulduktan sonra değiştirilemez.
// TableRows sadece Word'de dolu. Okunamayan belgede IsReadable false olur.
public sealed class ParsedDocument
{
    public bool IsReadable { get; }
    public string? UnreadableReason { get; }
    public IReadOnlyList<string> Lines { get; }
    public IReadOnlyList<IReadOnlyList<string>> TableRows { get; }

    private ParsedDocument(bool isReadable, string? reason,
        IReadOnlyList<string> lines, IReadOnlyList<IReadOnlyList<string>> tableRows)
    {
        IsReadable = isReadable;
        UnreadableReason = reason;
        Lines = lines;
        TableRows = tableRows;
    }

    public static ParsedDocument Readable(
        IEnumerable<string> lines, IEnumerable<IReadOnlyList<string>>? tableRows = null)
    {
        var cleanLines = lines.Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        var rows = tableRows?.ToList() ?? new List<IReadOnlyList<string>>();
        return new ParsedDocument(true, null, cleanLines, rows);
    }

    public static ParsedDocument NotReadable(string reason) =>
        new(false, reason, Array.Empty<string>(), Array.Empty<IReadOnlyList<string>>());
}