using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _db;

    public DocumentRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(Document document)
    {
        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        // Yanıttaki kullanıcı adı (username) dolu gelsin diye belgenin sahibi de yüklenir.
        await _db.Entry(document).Reference(d => d.User).LoadAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}