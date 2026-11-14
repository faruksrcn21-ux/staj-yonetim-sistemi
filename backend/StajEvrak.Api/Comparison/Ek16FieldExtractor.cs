using StajEvrak.Api.Parsing;

namespace StajEvrak.Api.Comparison;

// Ek-16'da öğrenci bilgileri dört sütunlu tablodadır (etiket | değer | etiket | değer);
// her değer solundaki en yakın etikete aittir. Staj yeri bölümünde de "Telefon No" gibi
// etiketler olduğu için sadece öğrenci bölümüne bakılır.
public class Ek16FieldExtractor
{
    // Bu kadar yakın X değerleri aynı sütun sayılır.
    private const double ColumnTolerance = 4;

    // Değer, etiketinden en fazla bu kadar yukarıda olabilir (aynı satırdaki küçük kaymalar).
    private const double RowTolerance = 3;

    private sealed record Label(string Variant, Ek16Field[] Fields);

    // Uzun yazılış önce denenir; böylece "t.c. kimlik no / uyruğu", "t.c. kimlik no" sanılmaz.
    private static readonly List<Label> Labels = Ek16Fields.All
        .SelectMany(f => f.LabelVariants.Select(v => new Label(v, new[] { f })))
        .Concat(Ek16Fields.Combined.Select(c => new Label(Ek16Field.Simplify(c.Label), c.Fields)))
        .OrderByDescending(l => l.Variant.Length)
        .ToList();

    public Dictionary<string, string> Extract(ParsedDocument parsed)
    {
        var found = new Dictionary<string, string>();
        if (!parsed.IsReadable)
            return found;

        var section = StudentSection(parsed.Cells).OrderBy(c => c.Y).ThenBy(c => c.X).ToList();
        var labels = new List<(TextCell Cell, Label Label, int End)>();
        var values = new List<TextCell>();
        var labelParts = new HashSet<TextCell>();

        foreach (var cell in section)
        {
            if (labelParts.Contains(cell))
                continue;

            var match = MatchWrappedLabel(cell, section, labelParts) ?? MatchLabelAt(cell.Text);
            if (match is null)
                values.Add(cell);
            else
                labels.Add((cell, match.Value.Label, match.Value.End));
        }

        var assigned = labels.ToDictionary(l => l.Cell, _ => new List<TextCell>());
        foreach (var value in values)
        {
            // Etiket sütunundaki metinler (ör. sağlık seçeneklerinin açıklamaları) değer değildir.
            if (labels.Any(l => Math.Abs(l.Cell.X - value.X) <= ColumnTolerance))
                continue;

            var owner = OwnerOf(value, labels.Select(l => l.Cell).ToList());
            if (owner is not null)
                assigned[owner].Add(value);
        }

        foreach (var (cell, label, end) in labels)
        {
            var cellsOfLabel = assigned[cell];
            var raw = cellsOfLabel.Count > 0
                ? string.Join(" ", cellsOfLabel.OrderBy(c => c.Y).ThenBy(c => c.X).Select(c => c.Text))
                : Rest(cell.Text, end);

            if (label.Fields.Length == 1)
            {
                Add(found, label.Fields[0], raw);
                continue;
            }

            // Birleşik alan: "10000000146 / T.C." → kimlik no ve uyruk
            var parts = raw.Split('/', 2);
            for (var i = 0; i < label.Fields.Length && i < parts.Length; i++)
                Add(found, label.Fields[i], parts[i]);
        }

        return found;
    }

    // Dar sütunda alt satırlara bölünmüş etiketi birleştirir ("SAĞLIK YARDIM" / "STATÜSÜ (*)").
    private static (Label Label, int End)? MatchWrappedLabel(TextCell first, List<TextCell> cells, HashSet<TextCell> labelParts)
    {
        const double maxLineGap = 14;
        var single = MatchLabelAt(first.Text);
        var singleLength = single?.Label.Variant.Length ?? 0;

        var text = first.Text;
        var parts = new List<TextCell>();
        var lastY = first.Y;
        (Label Label, int End)? best = null;
        var bestParts = 0;

        for (var k = 0; k < 2; k++)
        {
            var next = cells.FirstOrDefault(c => !labelParts.Contains(c) && !parts.Contains(c)
                && Math.Abs(c.X - first.X) <= ColumnTolerance && c.Y > lastY && c.Y - lastY <= maxLineGap);
            if (next is null)
                break;

            parts.Add(next);
            lastY = next.Y;
            text += " " + next.Text;

            var match = MatchLabelAt(text);
            if (match is not null && match.Value.Label.Variant.Length > singleLength
                && Ek16Field.Simplify(Rest(text, match.Value.End)).Length == 0)
            {
                best = (match.Value.Label, first.Text.Length); // etiketin tamamı; hücrede değer kalmaz
                bestParts = parts.Count;
            }
        }

        if (best is null)
            return null;

        foreach (var part in parts.Take(bestParts))
            labelParts.Add(part);
        return best;
    }

    private static List<TextCell> StudentSection(IReadOnlyList<TextCell> cells)
    {
        var start = cells.FirstOrDefault(c => Ek16Field.Simplify(c.Text) == "ogrencinin")?.Y ?? double.MinValue;
        var end = cells
            .Where(c => c.Y > start && Ek16Field.Simplify(c.Text).StartsWith("staj yapilan yerin"))
            .Select(c => c.Y)
            .DefaultIfEmpty(double.MaxValue)
            .Min();
        return cells.Where(c => c.Y >= start && c.Y < end).ToList();
    }

    // Değerin solundaki en yakın etiket sütunu; o sütunda değerin hizasındaki veya üstündeki son etiket.
    private static TextCell? OwnerOf(TextCell value, List<TextCell> labels)
    {
        var left = labels
            .Where(l => l.X < value.X - ColumnTolerance && l.Y <= value.Y + RowTolerance)
            .ToList();
        if (left.Count == 0)
            return null;

        var columnX = left.Max(l => l.X);
        return left
            .Where(l => Math.Abs(l.X - columnX) <= ColumnTolerance)
            .OrderByDescending(l => l.Y)
            .First();
    }

    private static void Add(Dictionary<string, string> found, Ek16Field field, string raw)
    {
        var value = field.Clean(raw);
        if (value.Length > 0 && !found.ContainsKey(field.Key))
            found[field.Key] = value;
    }

    private static (Label Label, int End)? MatchLabelAt(string text)
    {
        foreach (var label in Labels)
        {
            var end = PrefixEnd(text, label.Variant);
            if (end >= 0)
                return (label, end);
        }
        return null;
    }

    private static int PrefixEnd(string text, string variant)
    {
        for (var i = 1; i <= text.Length; i++)
        {
            var simplified = Ek16Field.Simplify(text[..i]);
            if (simplified.Length > variant.Length)
                return -1;
            if (simplified == variant && (i == text.Length || !char.IsLetterOrDigit(text[i])))
                return i;
        }
        return -1;
    }

    private static string Rest(string text, int end) => text[end..].TrimStart(' ', '\t', ':', '-', '–', '(', '*', ')');
}
