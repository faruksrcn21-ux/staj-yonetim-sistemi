using System.Globalization;
using ClosedXML.Excel;
using StajEvrak.Api.Dtos;
using C = StajEvrak.Api.Readers.StudentDatasetColumns;

namespace StajEvrak.Api.Readers;

public class ExcelStudentDatasetReader : IStudentDatasetReader
{
    public bool SupportsExtension(string extension) => extension == ".xlsx";

    public IReadOnlyList<StudentRow> Read(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(1);

        if ((sheet.LastColumnUsed()?.ColumnNumber() ?? 0) < C.Count)
            throw new FormatException($"Dosyada en az {C.Count} sütun olmalıdır.");

        var rows = new List<StudentRow>();
        foreach (var row in sheet.RowsUsed().Skip(1)) // ilk satır başlık
        {
            rows.Add(new StudentRow(
                RowNumber: row.RowNumber(),
                FullName: Get(row, C.FullName),
                TcNo: Get(row, C.TcNo),
                Nationality: Get(row, C.Nationality),
                StudentNo: Get(row, C.StudentNo),
                BirthDate: Get(row, C.BirthDate),
                Address: Get(row, C.Address),
                Department: Get(row, C.Department),
                SubmissionYear: Get(row, C.SubmissionYear),
                PreviousInternshipDays: Get(row, C.PreviousInternshipDays),
                Email: Get(row, C.Email),
                Phone: Get(row, C.Phone),
                HealthInsuranceStatus: Get(row, C.HealthInsuranceStatus)));
        }
        return rows;
    }

    private static string? Get(IXLRow row, int column)
    {
        var cell = row.Cell(column + 1); // ClosedXML 1'den başlar
        if (cell.IsEmpty()) return null;

        // Excel tarihi gerçek tarih olarak tutabilir; metne çeviriyoruz.
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

        return cell.GetString().Trim();
    }
}