using StajEvrak.Api.Models;

namespace StajEvrak.Api.Dtos;

// Dosyadan okunan ham satır, henüz doğrulanmadı.
public sealed record StudentRow(
    int RowNumber,
    string? FullName,
    string? TcNo,
    string? Nationality,
    string? StudentNo,
    string? BirthDate,
    string? Address,
    string? Department,
    string? SubmissionYear,
    string? PreviousInternshipDays,
    string? Email,
    string? Phone,
    string? HealthInsuranceStatus);

public sealed record RowError(int RowNumber, string Reason);

// Kapsülleme: sayaçlar sadece Record metotlarıyla değişir, hata listesi dışarıya salt okunur verilir.
public sealed class ImportResult
{
    private readonly List<RowError> _errors = new();

    public int Added { get; private set; }
    public int Updated { get; private set; }
    public int ErrorCount => _errors.Count;
    public IReadOnlyList<RowError> Errors => _errors;

    public void RecordAdded() => Added++;
    public void RecordUpdated() => Updated++;
    public void RecordError(int rowNumber, string reason) => _errors.Add(new RowError(rowNumber, reason));
}

// Kimlik no hassas veri olduğu için maskeli verilir.
public record DatasetRecordResponse(
    int Id,
    string FullName,
    string MaskedTcNo,
    string Nationality,
    string StudentNo,
    DateOnly BirthDate,
    string Address,
    string Department,
    string SubmissionYear,
    int PreviousInternshipDays,
    string Email,
    string Phone,
    string HealthInsuranceStatus)
{
    public static DatasetRecordResponse From(AdminTestDataset d) => new(
        d.Id, d.FullName, Mask(d.TcNo), d.Nationality, d.StudentNo, d.BirthDate, d.Address,
        d.Department, d.SubmissionYear, d.PreviousInternshipDays, d.Email, d.Phone,
        d.HealthInsuranceStatus);

    // 12345678901 -> 123******01
    private static string Mask(string tcNo) =>
        tcNo.Length <= 5 ? new string('*', tcNo.Length)
                         : tcNo[..3] + new string('*', tcNo.Length - 5) + tcNo[^2..];
}