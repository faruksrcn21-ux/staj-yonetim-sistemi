using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Repositories;

public class TestDatasetRepository : ITestDatasetRepository
{
    private readonly AppDbContext _db;

    public TestDatasetRepository(AppDbContext db) => _db = db;

    public Task<AdminTestDataset?> FindByTcNoAsync(string tcNo) =>
        _db.AdminTestDataset.FirstOrDefaultAsync(d => d.TcNo == tcNo);

    // Her kayıt kendi SaveChanges'iyle kaydedilir (satır satır): bir satırın hatası diğerlerini geri almaz.
    public async Task AddAsync(AdminTestDataset record)
    {
        _db.AdminTestDataset.Add(record);
        await SaveOrDetachAsync(record);
    }

    // Kayıt zaten takip ediliyor (FindByTcNoAsync ile okundu); sadece değişiklikleri kaydet.
    public Task UpdateAsync(AdminTestDataset record) => SaveOrDetachAsync(record);

    public async Task<IReadOnlyList<AdminTestDataset>> GetAllAsync() =>
        await _db.AdminTestDataset.AsNoTracking().OrderBy(d => d.Id).ToListAsync();

    private async Task SaveOrDetachAsync(AdminTestDataset record)
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Başarısız kayıt takipte kalırsa sonraki satırların kaydını da bozar.
            _db.Entry(record).State = EntityState.Detached;
            throw new DatasetStorageException("Veritabanına kaydedilemedi.", ex);
        }
    }
}