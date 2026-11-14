namespace StajEvrak.Api.Models;

/// <summary>
/// Öğrencinin yüklediği Ek-16 belgesi (documents tablosu).
/// Sütunlar ortak ER diyagramındaki (docs/01_VERITABANI_ER_DIYAGRAMI.md, C bölümü) ile aynıdır.
///
/// Kapsülleme: Alanlar dışarıdan doğrudan değiştirilemez (private set).
/// Belge her zaman "Yüklendi" statüsüyle oluşur; statü değişiklikleri ileride bu sınıfın
/// metotlarıyla yapılacak (izin verilmeyen geçişler engellenecek).
///
/// Not: Dosyanın yüklenmesi ve uzantı kontrolü (4.1), belgeden okuma (parsed_data) ve
/// karşılaştırma (manual_form_data, is_ready_for_submission, comparison_report) 2. geliştiricidedir.
/// </summary>
public class Document
{
    public int Id { get; private set; }

    /// <summary>Belgeyi yükleyen öğrenci.</summary>
    public int UserId { get; private set; }
    public BaseUser? User { get; private set; }

    /// <summary>Kullanıcının yüklediği dosyanın orijinal adı.</summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>Dosyanın sunucuda kaydedildiği yol.</summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>Dosya uzantısı (.pdf, .docx, .doc, .jpeg, .png).</summary>
    public string FileExtension { get; private set; } = string.Empty;

    /// <summary>Belgenin statüsü (bkz. <see cref="DocumentStatus"/>).</summary>
    public string Status { get; private set; } = DocumentStatus.Yuklendi;

    /// <summary>.docx / PDF kütüphanesiyle belgeden otomatik okunan veriler (JSON).</summary>
    public string? ParsedData { get; private set; }

    /// <summary>Öğrencinin arayüzdeki formdan elle girdiği veriler (JSON).</summary>
    public string? ManualFormData { get; private set; }

    /// <summary>Manuel veri, belge verisi ve test veri seti karşılaştırmasının sonucu.</summary>
    public bool IsReadyForSubmission { get; private set; }

    /// <summary>Uyuşmazlık varsa detay raporu.</summary>
    public string? ComparisonReport { get; private set; }

    public DateTime UploadedAt { get; private set; }

    // EF Core veritabanından okurken bu boş constructor'ı kullanır.
    private Document() { }

    /// <summary>Yeni yüklenen belge; statüsü her zaman "Yüklendi" olarak başlar.</summary>
    public Document(int userId, string fileName, string filePath, string fileExtension, DateTime uploadedAt)
    {
        UserId = userId;
        FileName = fileName;
        FilePath = filePath;
        FileExtension = fileExtension;
        Status = DocumentStatus.Yuklendi;
        UploadedAt = uploadedAt;
    }
}
