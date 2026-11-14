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
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}