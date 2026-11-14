namespace StajEvrak.Api.Models;

/// <summary>
/// Öğrencinin yüklediği Ek-16 belgesi (documents tablosu).
/// Sütunlar ortak ER diyagramındaki (docs/01_VERITABANI_ER_DIYAGRAMI.md, C bölümü) ile aynıdır.
///
/// Statü akışı (hoca, Bölüm 5):
///   Yüklendi → İnceleniyor → Hatalı / Eksik → (düzeltilip yeniden yüklenir) → Onaylandı → Tamamlandı
///
///   Yüklendi    → İnceleniyor  : StartReview()             (sistem, karşılaştırma başlarken)
///   İnceleniyor → Hatalı/Eksik : RecordComparisonResult()  (sistem, uyuşmazlık varsa)
///                                MarkDefective()           (admin elle; ör. okunamayan JPEG/PNG)
///   İnceleniyor → Onaylandı → Tamamlandı : Approve()       (admin onaylar, belge teslime hazır sayılır
///                                                           ve Tamamlandı'ya otomatik geçer)
///   Herhangi bir statü → Yüklendi : ReplaceFile()         (düzeltilip yeniden yükleme)
///
/// Kapsülleme: Alanlar dışarıdan doğrudan değiştirilemez (private set). Statü sadece
/// yukarıdaki metotlarla değişir; izin verilmeyen bir geçiş denenirse hata fırlatılır.
/// Böylece kural tek yerde kalır ve hiçbir servis statüyü yanlış bir değere atayamaz.
///
/// Not: Dosyanın yüklenmesi ve uzantı kontrolü (4.1), belgeden okuma ve karşılaştırma
/// 2. geliştiricidedir; karşılaştırma bitince StartReview / RecordComparisonResult çağrılır.
/// "Tamamlandı" belgesini kimin değiştirebileceği (S3) servis katmanında kontrol edilir.
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
    public Document(int userId, string fileName, string filePath, string fileExtension,
                    DateTime uploadedAt, string? manualFormData = null)
    {
        UserId = userId;
        FileName = fileName;
        FilePath = filePath;
        FileExtension = fileExtension;
        ManualFormData = manualFormData;
        Status = DocumentStatus.Yuklendi;
        UploadedAt = uploadedAt;
    }

    public bool IsCompleted => Status == DocumentStatus.Tamamlandi;

    /// <summary>Yüklendi → İnceleniyor. Karşılaştırma başlarken (veya admin incelemeye alırken) çağrılır.</summary>
    public void StartReview()
    {
        EnsureStatus(DocumentStatus.Yuklendi, "incelemeye alınamaz");
        Status = DocumentStatus.Inceleniyor;
    }

    /// <summary>
    /// Karşılaştırma sonucunu kaydeder (2. geliştirici çağırır).
    /// Belge hazır değilse statü İnceleniyor → Hatalı/Eksik olur.
    /// Hazırsa İnceleniyor'da kalır ve admin onayını bekler.
    /// </summary>
    public void RecordComparisonResult(string? parsedData, bool isReady, string? report)
    {
        EnsureStatus(DocumentStatus.Inceleniyor, "karşılaştırma sonucu kaydedilemez");
        ParsedData = parsedData;
        IsReadyForSubmission = isReady;
        ComparisonReport = report;

        if (!isReady)
            Status = DocumentStatus.HataliEksik;
    }

    /// <summary>İnceleniyor → Hatalı/Eksik (admin elle işaretler; sebep rapora yazılır).</summary>
    public void MarkDefective(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Hatalı/Eksik işaretlemek için bir sebep yazılmalıdır.");

        EnsureStatus(DocumentStatus.Inceleniyor, "Hatalı/Eksik olarak işaretlenemez");
        IsReadyForSubmission = false;
        ComparisonReport = reason;
        Status = DocumentStatus.HataliEksik;
    }

    /// <summary>
    /// İnceleniyor → Onaylandı → Tamamlandı. Admin onaylar.
    /// Hoca: "Onaylanan belgeler Tamamlandı statüsüne geçer." Bu yüzden onaylanan belge
    /// aynı anda Tamamlandı olur.
    /// </summary>
    public void Approve()
    {
        EnsureStatus(DocumentStatus.Inceleniyor, "onaylanamaz");
        Status = DocumentStatus.Onaylandi;
        IsReadyForSubmission = true; // admin onayladıysa belge teslime hazırdır
        Complete();
    }

    /// <summary>
    /// Düzeltilip yeniden yükleme: yeni dosya kaydedilir, eski okuma/karşılaştırma sonuçları
    /// silinir ve belge tekrar "Yüklendi" olur.
    /// "Tamamlandı" belgesinde bunu sadece admin yapabilir; bu kontrol servis katmanındadır.
    /// </summary>
    public void ReplaceFile(string fileName, string filePath, string fileExtension,
                            DateTime uploadedAt, string? manualFormData = null)
    {
        FileName = fileName;
        FilePath = filePath;
        FileExtension = fileExtension;
        ManualFormData = manualFormData;
        UploadedAt = uploadedAt;

        ParsedData = null;
        IsReadyForSubmission = false;
        ComparisonReport = null;
        Status = DocumentStatus.Yuklendi;
    }

    // Onaylandı → Tamamlandı (sadece Approve() içinden).
    private void Complete()
    {
        EnsureStatus(DocumentStatus.Onaylandi, "tamamlanamaz");
        Status = DocumentStatus.Tamamlandi;
    }

    private void EnsureStatus(string expected, string action)
    {
        if (Status != expected)
            throw new InvalidOperationException($"Belge '{Status}' statüsünde olduğu için {action}.");
    }
}
