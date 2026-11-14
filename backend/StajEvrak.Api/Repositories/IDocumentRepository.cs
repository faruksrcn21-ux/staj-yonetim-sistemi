using StajEvrak.Api.Models;

namespace StajEvrak.Api.Repositories;

public interface IDocumentRepository
{
    Task AddAsync(Document document);
    Task SaveChangesAsync();
}