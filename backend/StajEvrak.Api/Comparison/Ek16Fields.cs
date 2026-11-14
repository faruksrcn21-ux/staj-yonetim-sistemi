using StajEvrak.Api.Models;

namespace StajEvrak.Api.Comparison;

// Test veri setindeki 12 sütunun karşılaştırma kuralları.
// Anahtarlar frontend'deki form alanlarıyla aynıdır (manualFormData bu anahtarlarla gelir).
// Belgede bir alan bulunamıyorsa yeni etiket yazılışı buraya eklenir.
public static class Ek16Fields
{
    public static readonly IReadOnlyList<Ek16Field> All = new Ek16Field[]
    {
        new TextField("adSoyad", "Ad soyad",
            "adı soyadı", "ad soyad", "adı ve soyadı", "öğrencinin adı soyadı", "isim soyisim"),
        new DigitsField("tcNo", "TC kimlik no",
            "t.c. kimlik no", "tc kimlik no", "t.c. kimlik numarası", "tc kimlik numarası",
            "yabancı kimlik no", "kimlik no", "kimlik numarası"),
        new TextField("uyruk", "Uyruk", "uyruğu", "uyruk"),
        new DigitsField("ogrenciNo", "Öğrenci no", "öğrenci no", "öğrenci numarası", "okul no"),
        new DateField("dogumTarihi", "Doğum tarihi", "doğum tarihi"),
        new TextField("adres", "Adres",
            "ikametgah adresi", "ikametgâh adresi", "ikamet adresi", "açık adres", "ev adresi", "adres"),
        new TextField("bolum", "Bölüm", "bölümü", "bölüm", "programı"),
        new TextField("teslimYili", "Ek-16 teslim yılı",
            "eğitim öğretim yılı", "öğretim yılı", "teslim yılı", "akademik yıl"),
        new DigitsField("eskiStajGun", "Önceki staj gün sayısı",
            "daha önce yapılan staj gün sayısı", "önceki staj gün sayısı", "daha önce yapılan staj"),
        new EmailField("email", "E-posta", "e-posta adresi", "e-posta", "eposta", "e-mail", "email"),
        new PhoneField("telefon", "Telefon no", "cep telefonu", "telefon no", "telefon numarası", "telefon", "gsm"),
        new TextField("saglikYardimi", "Sağlık yardımı statüsü",
            "sağlık yardımı statüsü", "sağlık yardımı", "sağlık güvencesi", "sağlık sigortası"),
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
