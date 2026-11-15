namespace StajEvrak.Api.Storage;

// Soyutlama: servis dosyanın nerede saklandığını bilmez.
public interface IDocumentFileStore
{
    Task<string> SaveAsync(Stream content, string extension);
    void Delete(string filePath);
}