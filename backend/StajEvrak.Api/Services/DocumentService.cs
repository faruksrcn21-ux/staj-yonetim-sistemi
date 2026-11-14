using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

/// <summary>Belge işleminin sonucu; controller bunu HTTP durum koduna çevirir.</summary>
public enum DocumentOperationStatus
{
    Success,
    NotFound,          // 404: belge yok
    NotOwner,          // 403: başkasının belgesi
    Completed,         // 403: öğrenci "Tamamlandı" belgeyi değiştirmeye çalışıyor (S3)
    InvalidTransition, // 409: statü akışına aykırı geçiş (ör. Hatalı/Eksik belgeyi onaylamak)
}

public record DocumentOperationResult(DocumentOperationStatus Status, Document? Document = null, string? Message = null);

/// <summary>
/// Belge listeleme, silme ve statü değiştirme işlemleri (hoca Bölüm 4.2, 5 ve S3).
///
/// "Kim neye erişebilir" kuralı kullanıcı sınıflarındadır (BaseUser.CheckDocumentAccess,
/// çok biçimlilik); statü geçiş kuralları Document sınıfındadır (kapsülleme).
/// Bu servis sadece ikisini veritabanıyla birleştirir.
///
/// Not: Dosya yükleme / yeniden yükleme endpoint'leri (4.1) 2. geliştiricidedir.
/// Orada da yetki için <see cref="CheckAccessAsync"/> kullanılmalıdır.
/// </summary>
public class DocumentService
{
    private readonly AppDbContext _db;

    public DocumentService(AppDbContext db) => _db = db;

    /// <summary>Öğrencinin kendi belgeleri (en yeni en üstte).</summary>
    public Task<List<Document>> GetByUserAsync(int userId) =>
        _db.Documents
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.Id)
            .ToListAsync();

    /// <summary>
    /// Admin için tüm belgeler. status verilirse sadece o statüdekiler
    /// (ör. "TAMAMLANDI" → admin panelindeki "Tamamlandı" sekmesi, hoca 4.7).
    /// </summary>
    public Task<List<Document>> GetAllAsync(string? status)
    {
        var query = _db.Documents.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.Status == status);

        return query.OrderByDescending(d => d.Id).ToListAsync();
    }

    /// <summary>
    /// Belgeyi bulur ve kullanıcının erişim yetkisini kontrol eder.
    /// modify = true: değiştirme/silme, false: görüntüleme.
    /// </summary>
    public async Task<DocumentOperationResult> CheckAccessAsync(int documentId, int userId, bool modify)
    {
        var document = await _db.Documents.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == documentId);
        if (document is null)
            return new(DocumentOperationStatus.NotFound, Message: "Belge bulunamadı.");

        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return new(DocumentOperationStatus.NotOwner, Message: "Bu belgeye erişim yetkiniz yok.");

        // Çok biçimlilik: Student ve Admin bu soruya farklı cevap verir.
        return user.CheckDocumentAccess(document, modify) switch
        {
            DocumentAccess.NotOwner => new(DocumentOperationStatus.NotOwner,
                Message: "Bu belgeye erişim yetkiniz yok."),
            DocumentAccess.Completed => new(DocumentOperationStatus.Completed,
                Message: "\"Tamamlandı\" statüsündeki belge değiştirilemez veya silinemez."),
            _ => new(DocumentOperationStatus.Success, document),
        };
    }

    /// <summary>Belgeyi siler (öğrenci: kendi ve Tamamlandı olmayan; admin: hepsi).</summary>
    public async Task<DocumentOperationResult> DeleteAsync(int documentId, int userId)
    {
        var access = await CheckAccessAsync(documentId, userId, modify: true);
        if (access.Status != DocumentOperationStatus.Success)
            return access;

        // Not: Sunucudaki dosyanın kendisi 4.1'de (2. geliştirici) nereye kaydediliyorsa
        // oradan silinmelidir; burada sadece veritabanı kaydı silinir.
        _db.Documents.Remove(access.Document!);
        await _db.SaveChangesAsync();
        return access;
    }

    /// <summary>Admin: Yüklendi → İnceleniyor.</summary>
    public Task<DocumentOperationResult> StartReviewAsync(int documentId) =>
        ChangeStatusAsync(documentId, d => d.StartReview());

    /// <summary>Admin: İnceleniyor → Onaylandı → Tamamlandı.</summary>
    public Task<DocumentOperationResult> ApproveAsync(int documentId) =>
        ChangeStatusAsync(documentId, d => d.Approve());

    /// <summary>Admin: İnceleniyor → Hatalı/Eksik (sebep raporda görünür).</summary>
    public Task<DocumentOperationResult> MarkDefectiveAsync(int documentId, string reason) =>
        ChangeStatusAsync(documentId, d => d.MarkDefective(reason));

    private async Task<DocumentOperationResult> ChangeStatusAsync(int documentId, Action<Document> change)
    {
        var document = await _db.Documents.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == documentId);
        if (document is null)
            return new(DocumentOperationStatus.NotFound, Message: "Belge bulunamadı.");

        try
        {
            change(document); // izin verilmeyen geçişte Document hata fırlatır
        }
        catch (InvalidOperationException ex)
        {
            return new(DocumentOperationStatus.InvalidTransition, document, ex.Message);
        }

        await _db.SaveChangesAsync();
        return new(DocumentOperationStatus.Success, document);
    }
}
