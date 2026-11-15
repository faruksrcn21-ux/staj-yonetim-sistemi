using StajEvrak.Api.Models;

namespace StajEvrak.Api.Repositories;

public class DatasetStorageException : Exception
{
    public DatasetStorageException(string message, Exception inner) : base(message, inner) { }
}

// Soyutlama: servis veritabanının ne olduğunu bilmez, sadece bu arayüzü kullanır.
public interface ITestDatasetRepository
{
    Task<AdminTestDataset?> FindByTcNoAsync(string tcNo);
    Task AddAsync(AdminTestDataset record);
    Task UpdateAsync(AdminTestDataset record);
    Task<IReadOnlyList<AdminTestDataset>> GetAllAsync();
}