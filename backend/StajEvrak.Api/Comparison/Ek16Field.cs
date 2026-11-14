using System.Globalization;
using System.Text;

namespace StajEvrak.Api.Comparison;

// Soyutlama + kalıtım: her alan türü değeri kendi kuralıyla normalleştirir.
// Çok biçimlilik: karşılaştıran kod hangi türle çalıştığını bilmeden Matches() çağırır.
public abstract class Ek16Field
{
    public string Key { get; }
    public string Label { get; }

    // Belgede bu alanı gösteren etiket yazılışları (Türkçe karakterler sadeleştirilmiş hâlde).
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
