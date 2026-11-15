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
        // Kullanıcının dosya adı diske yazılmaz (yol manipülasyonu riski); rastgele ad verilir.
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