using StajEvrak.Api.Models;

namespace StajEvrak.Api.Repositories;

// Veri tabanı hatasını servise SQLite/EF'e bağımlı olmadan iletir
public class DatasetStorageException : Exception
{
    public DatasetStorageException(string message, Exception inner) : base(message, inner) { }
}

//Soyutlama: Servis veri tabanının SQLite olduğunu bilmez, sadece bu sözleşmeyi bilir.
public interface ITestDatasetRepository
{
    Task<AdminTestDataset?> FindByTcNoAsync(string tcNo);
    Task AddAsync(AdminTestDataset record);
    Task UpdateAsync(AdminTestDataset record);
    Task<IReadOnlyList<AdminTestDataset>> GetAllAsync();
}