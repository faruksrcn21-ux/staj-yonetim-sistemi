using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using StajEvrak.Api.Dtos;
using C = StajEvrak.Api.Readers.StudentDatasetColumns;

namespace StajEvrak.Api.Readers;

public class CsvStudentDatasetReader : IStudentDatasetReader
{
    public bool SupportsExtension(string extension) => extension == ".csv";

    public IReadOnlyList<StudentRow> Read(Stream stream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            DetectDelimiter = true,   // Türkçe Excel ";" ile kaydeder; otomatik tanınır
            BadDataFound = null,
            MissingFieldFound = null
        };

        using var textReader = new StreamReader(stream);
        using var csv = new CsvReader(textReader, config);

        csv.Read();
        csv.ReadHeader();
        if (csv.HeaderRecord is null || csv.HeaderRecord.Length < C.Count)
            throw new FormatException($"Dosyada en az {C.Count} sütun olmalıdır.");

        var rows = new List<StudentRow>();
        while (csv.Read())
        {
            rows.Add(new StudentRow(
                RowNumber: csv.Parser.Row,
                FullName: Get(csv, C.FullName),
                TcNo: Get(csv, C.TcNo),
                Nationality: Get(csv, C.Nationality),
                StudentNo: Get(csv, C.StudentNo),
                BirthDate: Get(csv, C.BirthDate),
                Address: Get(csv, C.Address),
                Department: Get(csv, C.Department),
                SubmissionYear: Get(csv, C.SubmissionYear),
                PreviousInternshipDays: Get(csv, C.PreviousInternshipDays),
                Email: Get(csv, C.Email),
                Phone: Get(csv, C.Phone),
                HealthInsuranceStatus: Get(csv, C.HealthInsuranceStatus)));
        }
        return rows;
    }

    private static string? Get(CsvReader csv, int column) =>
        csv.TryGetField<string>(column, out var value) ? value?.Trim() : null;
}