using StajEvrak.Api.Parsing;

namespace StajEvrak.Api.Comparison;

// Okunan belgeden Ek-16 alanlarını etiketlerine bakarak çıkarır.
// Word tablosunda: "etiket hücresi → yanındaki değer hücresi" veya "Etiket: değer".
// PDF satırında: "Etiket: değer"; aynı satırda ikinci bir etiket varsa değer orada kesilir.
public class Ek16FieldExtractor
{
    // Uzun yazılış önce denenir; böylece "e-posta adresi", "adres" sanılmaz.
    private static readonly List<(Ek16Field Field, string Variant)> Labels = Ek16Fields.All
        .SelectMany(f => f.LabelVariants.Select(v => (f, v)))
        .OrderByDescending(x => x.v.Length)
        .ToList();

    public Dictionary<string, string> Extract(ParsedDocument parsed)
    {
        var found = new Dictionary<string, string>();
        if (!parsed.IsReadable)
            return found;

        foreach (var row in parsed.TableRows)
        {
            for (var i = 0; i < row.Count; i++)
            {
                var label = MatchLabelAt(row[i], 0);
                if (label is null)
                    continue;

                var value = Rest(row[i], label.Value.End);
                if (value.Length == 0 && i + 1 < row.Count && MatchLabelAt(row[i + 1], 0) is null)
                    value = row[i + 1];

                Add(found, label.Value.Field, value);
            }
        }

        var lines = parsed.Lines;
        for (var i = 0; i < lines.Count; i++)
        {
            // PDF tablosunda uzun etiket iki satıra bölünebilir; değer parçaların arasına düşer.
            var split = MatchSplitLabel(lines, i);
            if (split is not null)
            {
                var (field, last) = split.Value;
                var between = string.Join(" ", lines.Skip(i + 1).Take(last - i - 1));
                Add(found, field, between.Length > 0 ? between : ValueAround(lines, i, last));
                i = last;
                continue;
            }

            var line = lines[i];
            var label = MatchLabelAt(line, 0);
            if (label is null)
                continue;

            var rest = Rest(line, label.Value.End);
            if (rest.Length == 0)
            {
                // Değer etiketin satırında değil: hücre içinde üst/alt satırlara dağılmış olabilir.
                Add(found, label.Value.Field, ValueAround(lines, i, i));
                continue;
            }

            Add(found, label.Value.Field, CutAtNextLabel(rest));

            // Satırın devamında başka alanlar da olabilir (PDF'te iki sütun aynı satıra düşer).
            var next = NextLabelIndex(rest);
            while (next >= 0)
            {
                var other = MatchLabelAt(rest, next)!.Value;
                var otherRest = Rest(rest, other.End);
                Add(found, other.Field, CutAtNextLabel(otherRest));
                rest = otherRest;
                next = NextLabelIndex(rest);
            }
        }

        return found;
    }

    // lines[i] ile 1 veya 2 sonraki satır birlikte, tek satırdakinden daha uzun bir etiket oluşturuyor mu?
    private static (Ek16Field Field, int Last)? MatchSplitLabel(IReadOnlyList<string> lines, int i)
    {
        var single = MatchLabelAt(lines[i], 0);
        var singleLength = single is null ? 0 : Ek16Field.Simplify(lines[i][..single.Value.End]).Length;

        for (var last = i + 1; last <= i + 2 && last < lines.Count; last++)
        {
            var combined = lines[i] + " " + lines[last];
            var label = MatchLabelAt(combined, 0);
            if (label is null || Rest(combined, label.Value.End).Length > 0)
                continue;
            if (Ek16Field.Simplify(combined).Length > singleLength)
                return (label.Value.Field, last);
        }
        return null;
    }

    // Etiketin üstündeki ve altındaki etiketsiz satırlar. Hücre ortalandığında değer
    // etiketin iki yanına eşit dağılır; bu yüzden üstten en fazla alttaki kadar satır alınır.
    private static string ValueAround(IReadOnlyList<string> lines, int first, int last)
    {
        const int max = 3;
        var after = new List<string>();
        for (var k = last + 1; k < lines.Count && after.Count < max && MatchLabelAt(lines[k], 0) is null; k++)
            after.Add(lines[k]);

        var before = new List<string>();
        for (var k = first - 1; k >= 0 && before.Count < after.Count && MatchLabelAt(lines[k], 0) is null; k--)
            before.Insert(0, lines[k]);

        return string.Join(" ", before.Concat(after));
    }

    private static void Add(Dictionary<string, string> found, Ek16Field field, string raw)
    {
        var value = CleanValue(raw);
        if (value.Length > 0 && !found.ContainsKey(field.Key))
            found[field.Key] = value;
    }

    // Metnin start konumunda bir etiket başlıyor mu? Başlıyorsa etiketin bittiği konumu döner.
    private static (Ek16Field Field, int End)? MatchLabelAt(string text, int start)
    {
        foreach (var (field, variant) in Labels)
        {
            var end = PrefixEnd(text, start, variant);
            if (end >= 0)
                return (field, end);
        }
        return null;
    }

    // text[start..] sadeleştirildiğinde variant ile başlıyorsa, ham metinde etiketin bittiği konum.
    private static int PrefixEnd(string text, int start, string variant)
    {
        for (var i = start + 1; i <= text.Length; i++)
        {
            var simplified = Ek16Field.Simplify(text[start..i]);
            if (simplified.Length > variant.Length)
                return -1;
            if (simplified == variant && (i == text.Length || !char.IsLetterOrDigit(text[i])))
                return i;
        }
        return -1;
    }

    private static int NextLabelIndex(string text)
    {
        for (var i = 1; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i - 1]) && char.IsLetter(text[i]) && MatchLabelAt(text, i) is not null)
                return i;
        }
        return -1;
    }

    private static string CutAtNextLabel(string text)
    {
        var next = NextLabelIndex(text);
        return next < 0 ? text : text[..next];
    }

    private static string Rest(string text, int end) => text[end..].TrimStart(' ', '\t', ':', '-', '–');

    // Onay kutusu varsa işaretli (☒) seçenek alınır.
    private static string CleanValue(string raw)
    {
        var value = raw;
        var mark = value.IndexOf('☒');
        if (mark >= 0)
        {
            var after = value[(mark + 1)..];
            var stop = after.IndexOfAny(new[] { '☐', '☒' });
            value = stop >= 0 ? after[..stop] : after;
        }
        return value.Replace("☐", "").Replace("\n", " ").Trim(' ', '\t', ':', '-', '–', ',', ';');
    }
}
