using System.Globalization;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Readers;
using StajEvrak.Api.Repositories;

namespace StajEvrak.Api.Services;


public interface IDatasetImportService
{
    Task<ImportResult> ImportAsync(string fileName, Stream file);
    Task<IReadOnlyList<AdminTestDataset>> GetAllAsync();
}


public class DatasetImportService : IDatasetImportService
{
    private static readonly string[] DateFormats =
        { "dd.MM.yyyy", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" };

    private readonly DatasetReaderFactory _readers;
    private readonly ITestDatasetRepository _repository;

    public DatasetImportService(DatasetReaderFactory readers, ITestDatasetRepository repository)
    {
        _readers = readers;
        _repository = repository;
    }

    public async Task<ImportResult> ImportAsync(string fileName, Stream file)
    {
        // Çok biçimlilik: hangi reader seçilirse seçilsin burada sadece Read() çağrılır.
        var reader = _readers.Select(fileName);
        var rows = reader.Read(file);

        var result = new ImportResult();
        foreach (var row in rows)
        {
            try
            {
                await ImportRowAsync(row, result);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException or DatasetStorageException)
            {
                result.RecordError(row.RowNumber, ex.Message);
            }
        }
        return result;
    }

    public Task<IReadOnlyList<AdminTestDataset>> GetAllAsync() => _repository.GetAllAsync();

    private async Task<bool> ImportRowAsync(StudentRow row, ImportResult result)
    {
        var tcNo = AdminTestDataset.NormalizeTcNo(row.TcNo);
        var birthDate = ParseBirthDate(row.BirthDate);
        var internshipDays = ParseInternshipDays(row.PreviousInternshipDays);

        var existing = await _repository.FindByTcNoAsync(tcNo);
        if (existing is null)
        {
            var record = new AdminTestDataset(
                fullName: row.FullName, tcNo: tcNo, nationality: row.Nationality,
                studentNo: row.StudentNo, birthDate: birthDate, address: row.Address,
                department: row.Department, submissionYear: row.SubmissionYear,
                previousInternshipDays: internshipDays, email: row.Email, phone: row.Phone,
                healthInsuranceStatus: row.HealthInsuranceStatus);

            await _repository.AddAsync(record);
            result.RecordAdded();
        }
        else
        {
            existing.UpdateFrom(
                fullName: row.FullName, nationality: row.Nationality, studentNo: row.StudentNo,
                birthDate: birthDate, address: row.Address, department: row.Department,
                submissionYear: row.SubmissionYear, previousInternshipDays: internshipDays,
                email: row.Email, phone: row.Phone, healthInsuranceStatus: row.HealthInsuranceStatus);

            await _repository.UpdateAsync(existing);
            result.RecordUpdated();
        }
        return true;
    }

    private static DateOnly ParseBirthDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Doğum tarihi boş olamaz.");

        if (DateOnly.TryParseExact(value.Trim(), DateFormats, CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out var date))
            return date;

        throw new FormatException($"Doğum tarihi okunamadı: '{value}'.");
    }

    private static int ParseInternshipDays(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0; // boşsa daha önce staj yapmamış sayılır

        if (int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var days))
            return days;

        throw new FormatException($"Staj gün sayısı sayı olmalıdır: '{value}'.");
    }
}