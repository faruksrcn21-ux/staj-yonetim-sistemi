using StajEvrak.Api.Dtos;

namespace StajEvrak.Api.Readers;

//"Bir dosyadan öğrenci satırları oku" sözleşmesi. Servis hangi formatın okunduğunu bilmez.
public interface IStudentDatasetReader
{
    bool SupportsExtension(string extension);   // ör. ".csv"
    IReadOnlyList<StudentRow> Read(Stream stream);
}