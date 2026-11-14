namespace StajEvrak.Api.Storage;

public class LocalDocumentFileStore : IDocumentFileStore
{
    private readonly string _root;

    public LocalDocumentFileStore(IHostEnvironment environment)
    {
        _root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "uploads"));
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string extension)
    {
        // Güvenlik: kullanıcının verdiği dosya adı diske yazılmaz (yol manipülasyonu ve çakışma riski). Rastgele bir ad kullanılır, orijinal ad sadece veri tabanında durur.
        var path = Path.Combine(_root, $"{Guid.NewGuid():N}{extension}");

        await using var target = File.Create(path);
        await content.CopyToAsync(target);
        return path;
    }

    public void Delete(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);

        // Sadece uploads klasörünün içindeki dosyalar silinir.
        if (fullPath.StartsWith(_root, StringComparison.Ordinal) && File.Exists(fullPath))
            File.Delete(fullPath);
    }
}