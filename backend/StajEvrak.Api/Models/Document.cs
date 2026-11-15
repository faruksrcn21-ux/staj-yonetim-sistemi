namespace StajEvrak.Api.Models;

// Statü akışı: Yüklendi → İnceleniyor → Hatalı/Eksik → (yeniden yükleme) → Onaylandı → Tamamlandı
// Kapsülleme: statü dışarıdan atanamaz, sadece aşağıdaki metotlarla değişir.
// Akışa uymayan bir geçiş denenirse hata fırlatılır.
public class Document
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public BaseUser? User { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public string Status { get; private set; } = DocumentStatus.Yuklendi;
    public string? ParsedData { get; private set; }
    public string? ManualFormData { get; private set; }
    public bool IsReadyForSubmission { get; private set; }
    public string? ComparisonReport { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private Document() { }

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

    public void StartReview()
    {
        EnsureStatus(DocumentStatus.Yuklendi, "incelemeye alınamaz");
        Status = DocumentStatus.Inceleniyor;
    }

    // Belge hazır değilse Hatalı/Eksik olur, hazırsa admin onayını bekler.
    public void RecordComparisonResult(string? parsedData, bool isReady, string? report)
    {
        EnsureStatus(DocumentStatus.Inceleniyor, "karşılaştırma sonucu kaydedilemez");
        ParsedData = parsedData;
        IsReadyForSubmission = isReady;
        ComparisonReport = report;

        if (!isReady)
            Status = DocumentStatus.HataliEksik;
    }

    public void MarkDefective(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Hatalı/Eksik işaretlemek için bir sebep yazılmalıdır.");

        EnsureStatus(DocumentStatus.Inceleniyor, "Hatalı/Eksik olarak işaretlenemez");
        IsReadyForSubmission = false;
        ComparisonReport = reason;
        Status = DocumentStatus.HataliEksik;
    }

    // Onaylanan belge teslime hazır sayılır ve hemen Tamamlandı olur.
    public void Approve()
    {
        EnsureStatus(DocumentStatus.Inceleniyor, "onaylanamaz");
        Status = DocumentStatus.Onaylandi;
        IsReadyForSubmission = true;
        Complete();
    }

    // Yeniden yüklemede eski sonuçlar silinir ve belge tekrar Yüklendi olur.
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
