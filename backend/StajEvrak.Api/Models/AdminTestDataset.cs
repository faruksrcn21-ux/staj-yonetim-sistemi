namespace StajEvrak.Api.Models;

// Admin'in yüklediği test veri setindeki bir öğrenci kaydı (admin_test_dataset tablosu).
// Sütunlar hocanın dokümanındaki 12 sütunla birebir aynıdır.
//
// Kapsülleme: Setter'lar private. Kayıt sadece constructor ile oluşturulur ve
// UpdateFrom ile güncellenir; doğrulama kuralları bu iki yolun ortak noktasında
// (Apply) tek yerde durur. Kimlik no (TcNo) kaydı bulmak için kullanılan anahtar
// olduğundan güncellemede değişmez.
//
public class AdminTestDataset
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string TcNo { get; private set; } = string.Empty;
    public string Nationality { get; private set; } = string.Empty;
    public string StudentNo { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string Department { get; private set; } = string.Empty;
    public string SubmissionYear { get; private set; } = string.Empty;
    public int PreviousInternshipDays { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string HealthInsuranceStatus { get; private set; } = string.Empty;

    // EF Core veritabanından okurken bu boş constructor'ı kullanır.
    private AdminTestDataset() { }

    public AdminTestDataset(
        string? fullName, string? tcNo, string? nationality, string? studentNo,
        DateOnly birthDate, string? address, string? department, string? submissionYear,
        int previousInternshipDays, string? email, string? phone, string? healthInsuranceStatus)
    {
        TcNo = NormalizeTcNo(tcNo);
        Apply(fullName, nationality, studentNo, birthDate, address, department,
              submissionYear, previousInternshipDays, email, phone, healthInsuranceStatus);
    }

    //Mevcut kaydı yeni bilgilerle günceller (upsert'in "güncelle" kolu).
    public void UpdateFrom(
        string? fullName, string? nationality, string? studentNo,
        DateOnly birthDate, string? address, string? department, string? submissionYear,
        int previousInternshipDays, string? email, string? phone, string? healthInsuranceStatus)
    {
        Apply(fullName, nationality, studentNo, birthDate, address, department,
              submissionYear, previousInternshipDays, email, phone, healthInsuranceStatus);
    }

    //Kimlik no zorunludur; kayıt aranmadan önce de bu kural uygulanır.
    public static string NormalizeTcNo(string? tcNo) => Required(tcNo, "Kimlik no");

    // Önce HEPSİ doğrulanır, sonra atanır: hatalı satır kaydı yarım güncellemez.
    private void Apply(
        string? fullName, string? nationality, string? studentNo,
        DateOnly birthDate, string? address, string? department, string? submissionYear,
        int previousInternshipDays, string? email, string? phone, string? healthInsuranceStatus)
    {
        var validFullName = Required(fullName, "Ad soyad");
        var validStudentNo = Required(studentNo, "Öğrenci no");

        if (previousInternshipDays < 0)
            throw new ArgumentException("Staj gün sayısı negatif olamaz.");

        FullName = validFullName;
        StudentNo = validStudentNo;
        Nationality = Clean(nationality);
        BirthDate = birthDate;
        Address = Clean(address);
        Department = Clean(department);
        SubmissionYear = Clean(submissionYear);
        PreviousInternshipDays = previousInternshipDays;
        Email = Clean(email);
        Phone = Clean(phone);
        HealthInsuranceStatus = Clean(healthInsuranceStatus);
    }

    private static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} boş olamaz.");
        return value.Trim();
    }

    private static string Clean(string? value) => value?.Trim() ?? string.Empty;
}