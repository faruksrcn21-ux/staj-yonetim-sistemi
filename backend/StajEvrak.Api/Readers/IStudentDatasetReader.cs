using StajEvrak.Api.Dtos;

namespace StajEvrak.Api.Readers;

// Soyutlama: servis hangi dosya formatının okunduğunu bilmez.
public interface IStudentDatasetReader
{
    bool SupportsExtension(string extension);
    IReadOnlyList<StudentRow> Read(Stream stream);
}