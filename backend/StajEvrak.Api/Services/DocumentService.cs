using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;
using StajEvrak.Api.Storage;

namespace StajEvrak.Api.Services;

public enum DocumentOperationStatus
{
    Success,
    NotFound,          // 404
    NotOwner,          // 403
    Completed,         // 403
    InvalidTransition, // 409
}

public record DocumentOperationResult(DocumentOperationStatus Status, Document? Document = null, string? Message = null);

// Erişim kuralı kullanıcı sınıflarında, statü kuralları Document'ta;
// bu servis ikisini veritabanıyla birleştirir.
public class DocumentService
{
    private readonly AppDbContext _db;
    private readonly IDocumentFileStore _files;

    public DocumentService(AppDbContext db, IDocumentFileStore files)
    {
        _db = db;
        _files = files;
    }

    public Task<List<Document>> GetByUserAsync(int userId) =>
        _db.Documents
            .Include(d => d.User)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.Id)
            .ToListAsync();

    // status verilirse sadece o statüdeki belgeler döner.
    public Task<List<Document>> GetAllAsync(string? status)
    {
        var query = _db.Documents.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.Status == status);

        return query.OrderByDescending(d => d.Id).ToListAsync();
    }

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

    public async Task<DocumentOperationResult> DeleteAsync(int documentId, int userId)
    {
        var access = await CheckAccessAsync(documentId, userId, modify: true);
        if (access.Status != DocumentOperationStatus.Success)
            return access;

        var filePath = access.Document!.FilePath;
        _db.Documents.Remove(access.Document!);
        await _db.SaveChangesAsync();

        // Dosya, kayıt silindikten sonra silinir; kayıt silinemezse dosya yerinde kalır.
        _files.Delete(filePath);
        return access;
    }

    public Task<DocumentOperationResult> StartReviewAsync(int documentId) =>
        ChangeStatusAsync(documentId, d => d.StartReview());

    public Task<DocumentOperationResult> ApproveAsync(int documentId) =>
        ChangeStatusAsync(documentId, d => d.Approve());

    public Task<DocumentOperationResult> MarkDefectiveAsync(int documentId, string reason) =>
        ChangeStatusAsync(documentId, d => d.MarkDefective(reason));

    private async Task<DocumentOperationResult> ChangeStatusAsync(int documentId, Action<Document> change)
    {
        var document = await _db.Documents.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == documentId);
        if (document is null)
            return new(DocumentOperationStatus.NotFound, Message: "Belge bulunamadı.");

        try
        {
            change(document);
        }
        catch (InvalidOperationException ex)
        {
            return new(DocumentOperationStatus.InvalidTransition, document, ex.Message);
        }

        await _db.SaveChangesAsync();
        return new(DocumentOperationStatus.Success, document);
    }
}
