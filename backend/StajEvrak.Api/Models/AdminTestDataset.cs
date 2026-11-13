namespace StajEvrak.Api.Models;

/// <summary>
/// Admin'in yüklediği test veri setindeki bir öğrenci kaydı (admin_test_dataset tablosu).
/// Sütunlar hocanın dokümanındaki (Bölüm 4.3) 12 sütunla birebir aynıdır.
///
/// Not: Bu tabloyu 1. geliştirici oluşturur; CSV/Excel'den içe aktarma ve
/// karşılaştırma işi 2. geliştiricidedir.
/// </summary>
public class AdminTestDataset
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;         // Öğrenci ad soyad
    public string TcNo { get; set; } = string.Empty;             // TC / yabancı kimlik no
    public string Nationality { get; set; } = string.Empty;      // Uyruk
    public string StudentNo { get; set; } = string.Empty;        // Öğrenci no
    public DateOnly BirthDate { get; set; }                      // Doğum tarihi
    public string Address { get; set; } = string.Empty;          // İkametgâh adresi (açık adres)
    public string Department { get; set; } = string.Empty;       // Okuduğu bölüm
    public string SubmissionYear { get; set; } = string.Empty;   // Ek-16 teslim yılı (örn. 2025-2026)
    public int PreviousInternshipDays { get; set; }              // Daha önce yapılan staj gün sayısı
    public string Email { get; set; } = string.Empty;            // E-posta
    public string Phone { get; set; } = string.Empty;            // Telefon no
    public string HealthInsuranceStatus { get; set; } = string.Empty; // Sağlık yardımı statüsü
}
