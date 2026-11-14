using StajEvrak.Api.Models;

namespace StajEvrak.Api.Comparison;

// Test veri setindeki 12 sütunun karşılaştırma kuralları.
// Anahtarlar frontend'deki form alanlarıyla aynıdır (manualFormData bu anahtarlarla gelir).
// Etiketler KOÜ Mühendislik Fakültesi Ek-16 "Staj Kabul Formu"ndaki yazılışlardır;
// belgede bir alan bulunamıyorsa yeni yazılış buraya eklenir.
public static class Ek16Fields
{
    public static readonly Ek16Field TcNo = new DigitsField("tcNo", "TC kimlik no",
        "t.c. kimlik no", "tc kimlik no", "t.c. kimlik numarası", "tc kimlik numarası", "yabancı kimlik no", "kimlik no");
    public static readonly Ek16Field Nationality = new TextField("uyruk", "Uyruk", "uyruğu", "uyruk");
    public static readonly Ek16Field Email = new EmailField("email", "E-posta",
        "e-posta adresi", "e-posta", "eposta", "e-mail", "email");
    public static readonly Ek16Field Phone = new PhoneField("telefon", "Telefon no",
        "cep telefonu", "telefon no", "telefon numarası", "telefon", "gsm");

    public static readonly IReadOnlyList<Ek16Field> All = new[]
    {
        new TextField("adSoyad", "Ad soyad", "adı soyadı", "ad soyad", "adı ve soyadı"),
        TcNo,
        Nationality,
        new DigitsField("ogrenciNo", "Öğrenci no", "öğrenci numarası", "öğrenci no"),
        new DateField("dogumTarihi", "Doğum tarihi", "doğum tarihi"),
        new TextField("adres", "Adres", "ikametgâh adresi", "ikametgah adresi", "ikamet adresi", "adres"),
        new TextField("bolum", "Bölüm", "bölümü", "bölüm"),
        new AcademicYearField("teslimYili", "Öğretim yılı",
            "öğretim yılı", "eğitim öğretim yılı", "teslim yılı"),
        new DigitsField("eskiStajGun", "Önceki staj gün sayısı",
            "daha önce yaptığı staj gün sayısı", "daha önce yapılan staj gün sayısı", "önceki staj gün sayısı"),
        Email,
        Phone,
        new HealthStatusField("saglikYardimi", "Sağlık yardım statüsü",
            "sağlık yardım statüsü", "sağlık yardımı statüsü"),
    };

    // Ek-16'da iki alan tek hücrede durur; değer "/" işaretinden bölünür.
    public static readonly IReadOnlyList<(string Label, Ek16Field[] Fields)> Combined = new[]
    {
        ("t.c. kimlik no / uyruğu", new[] { TcNo, Nationality }),
        ("e-posta adresi / telefon no", new[] { Email, Phone }),
    };

    // Veri setindeki kaydı aynı anahtarlarla sözlüğe çevirir.
    public static Dictionary<string, string> FromDataset(AdminTestDataset d) => new()
    {
        ["adSoyad"] = d.FullName,
        ["tcNo"] = d.TcNo,
        ["uyruk"] = d.Nationality,
        ["ogrenciNo"] = d.StudentNo,
        ["dogumTarihi"] = d.BirthDate.ToString("yyyy-MM-dd"),
        ["adres"] = d.Address,
        ["bolum"] = d.Department,
        ["teslimYili"] = d.SubmissionYear,
        ["eskiStajGun"] = d.PreviousInternshipDays.ToString(),
        ["email"] = d.Email,
        ["telefon"] = d.Phone,
        ["saglikYardimi"] = d.HealthInsuranceStatus,
    };
}
