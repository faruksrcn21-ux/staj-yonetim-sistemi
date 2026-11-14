namespace StajEvrak.Api.Readers;

//Dosya uzantısına göre doğru reader'ı seçer.
public class DatasetReaderFactory
{
    private readonly IEnumerable<IStudentDatasetReader> _readers;

    // DI, kayıtlı tüm reader'ları buraya verir.
    public DatasetReaderFactory(IEnumerable<IStudentDatasetReader> readers) => _readers = readers;

    public IStudentDatasetReader Select(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _readers.FirstOrDefault(r => r.SupportsExtension(extension))
               ?? throw new NotSupportedException(
                   $"'{extension}' uzantısı desteklenmiyor. Kabul edilenler: .csv, .xlsx");
    }
}