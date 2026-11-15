using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StajEvrak.Api.Comparison;

// Soyutlama + kalıtım: her alan türü değeri kendi kuralıyla normalleştirir.
// Çok biçimlilik: karşılaştıran kod hangi türle çalıştığını bilmeden Matches() çağırır.
public abstract class Ek16Field
{
    public string Key { get; }
    public string Label { get; }

    public IReadOnlyList<string> LabelVariants { get; }

    protected Ek16Field(string key, string label, params string[] labelVariants)
    {
        Key = key;
        Label = label;
        LabelVariants = labelVariants.Select(Simplify).ToList();
    }

    protected abstract string Normalize(string value);

    public bool Matches(string? a, string? b) =>
        Normalize(a ?? string.Empty) == Normalize(b ?? string.Empty);

    // Belgeden okunan ham metni değere çevirir; değer yoksa boş döner.
    public virtual string Clean(string raw) =>
        raw.Replace("☐", "").Replace("\n", " ").Trim(' ', '\t', ':', '-', '–', ',', ';');

    public virtual string Display(string? value) => value ?? string.Empty;

    // Türkçe karakterleri sadeleştirir, küçük harfe çevirir, harf/rakam dışını boşluk yapar.
    public static string Simplify(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text.ToLower(new CultureInfo("tr-TR")))
        {
            sb.Append(ch switch
            {
                'ı' => 'i', 'ğ' => 'g', 'ü' => 'u', 'ş' => 's', 'ö' => 'o', 'ç' => 'c',
                'â' => 'a', 'î' => 'i', 'û' => 'u',
                _ => char.IsLetterOrDigit(ch) ? ch : ' ',
            });
        }
        return string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

// Ad, adres, bölüm gibi serbest metinler: büyük/küçük harf, Türkçe karakter ve noktalama farkı önemsenmez.
public class TextField : Ek16Field
{
    public TextField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value) => Simplify(value).Replace(" ", "");
}

// Kimlik no, öğrenci no, gün sayısı: sadece rakamlar karşılaştırılır.
public class DigitsField : Ek16Field
{
    public DigitsField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value) => new(value.Where(char.IsDigit).ToArray());
}

// +90 / 0 ön eki farkı önemsenmesin diye son 10 hane karşılaştırılır.
public class PhoneField : DigitsField
{
    public PhoneField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value)
    {
        var digits = base.Normalize(value);
        return digits.Length > 10 ? digits[^10..] : digits;
    }
}

// 01.02.2003, 1/2/2003 ve 2003-02-01 aynı tarih sayılır.
public class DateField : Ek16Field
{
    private static readonly string[] Formats =
        { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

    public DateField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value) =>
        DateOnly.TryParseExact(value.Trim(), Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.ToString("yyyy-MM-dd")
            : value.Trim();
}

public class EmailField : Ek16Field
{
    public EmailField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value) => value.Trim().ToLowerInvariant();
}

// Ek-16'da "2025-2026 Bahar" yazılır, veri setinde "2025-2026"; sadece yıllar karşılaştırılır.
public class AcademicYearField : Ek16Field
{
    private static readonly Regex Years = new(@"(\d{4})\s*[-–/]\s*(\d{4})");

    public AcademicYearField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value)
    {
        var match = Years.Match(value);
        return match.Success ? $"{match.Groups[1].Value}-{match.Groups[2].Value}" : Simplify(value);
    }
}

// Ek-16'daki tek seçimli "Sağlık yardım statüsü" (6 seçenek).
// Numara ("2"), seçeneğin metni veya işaretli onay kutusu aynı seçenek sayılır.
public class HealthStatusField : Ek16Field
{
    public static readonly IReadOnlyList<string> Options = new[]
    {
        "Genel Sağlık sigortası kapsamındayım",
        "Anne-Baba üzerinden sağlık yardımı almaktayım",
        "Sosyal Sigortalar Kurumu'na tabi olarak çalıştığımdan kendi sağlık yardımım var",
        "Bağ-Kur'a tabi olarak çalıştığımdan kendi sağlık yardımım var",
        "Emekli Sandığı'na tabi olarak çalıştığımdan kendi sağlık yardımım var",
        "Herhangi bir sosyal güvencem olmadığından sağlık yardımı alamıyorum",
    };

    private static readonly string[] Keywords =
        { "genel saglik", "anne baba", "sosyal sigortalar", "bag kur", "emekli sandigi", "sosyal guvencem" };

    private const string Boxes = "☐☒☑■";

    // "2-☒", "2 ☑" veya "2-☐ X" gibi işaretlenmiş kutu
    private static readonly Regex Marked = new(@"([1-6])\s*[-.)]?\s*(?:[☒☑■✓✔]|☐\s*[xX✓✔])");
    private static readonly Regex LeadingNumber = new(@"^\s*([1-6])(?!\d)");

    public HealthStatusField(string key, string label, params string[] labelVariants) : base(key, label, labelVariants) { }

    protected override string Normalize(string value) =>
        OptionNumber(value)?.ToString() ?? Simplify(value);

    public override string Clean(string raw)
    {
        // Onay kutuları varsa sadece işaretli olan geçerlidir; hiçbiri işaretli değilse değer yoktur.
        if (raw.IndexOfAny(Boxes.ToCharArray()) >= 0)
        {
            var marked = Marked.Match(raw);
            return marked.Success ? Options[int.Parse(marked.Groups[1].Value) - 1] : string.Empty;
        }

        // Seçeneklerden birine karşılık gelmeyen metin değer sayılmaz.
        var number = OptionNumber(raw);
        return number is null ? string.Empty : Options[number.Value - 1];
    }

    public override string Display(string? value)
    {
        var number = OptionNumber(value ?? string.Empty);
        return number is null ? value ?? string.Empty : Options[number.Value - 1];
    }

    private static int? OptionNumber(string value)
    {
        var marked = Marked.Match(value);
        if (marked.Success)
            return int.Parse(marked.Groups[1].Value);

        var leading = LeadingNumber.Match(value);
        if (leading.Success)
            return int.Parse(leading.Groups[1].Value);

        var simplified = Simplify(value);
        for (var i = 0; i < Keywords.Length; i++)
            if (simplified.Contains(Keywords[i]))
                return i + 1;
        return null;
    }
}
