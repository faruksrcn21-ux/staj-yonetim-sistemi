namespace StajEvrak.Api.Storage;

/// Soyutlama: Servis dosyanın diskte mi, bulutta mı durduğunu bilmez.
public interface IDocumentFileStore
{
    Task<string> SaveAsync(Stream content, string extension);   // kaydedilen yolu döner
    void Delete(string filePath);
}