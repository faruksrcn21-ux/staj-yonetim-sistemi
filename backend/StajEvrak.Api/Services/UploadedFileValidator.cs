namespace StajEvrak.Api.Services;

public sealed record FileValidationResult(bool IsValid, string Extension, string? Error)
{
    public static FileValidationResult Ok(string extension) => new(true, extension, null);
    public static FileValidationResult Fail(string error) => new(false, string.Empty, error);
}


/// Evrak yükleme format/uzantı kontrolü (hoca 4.4 ve S2). Kabul edilenler: Word (.doc/.docx),
/// PDF, JPEG, PNG. Diğerleri reddedilir ve kullanıcıya uyarı mesajı verilir.
/// Kapsülleme: Kabul listesi ve dosya imzaları sınıfın içinde private durur.

public class UploadedFileValidator
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private const string AcceptedFormats = ".doc, .docx, .pdf, .jpeg, .jpg, .png";

    // Uzantı → dosyanın ilk baytları (dosya imzası)
    private static readonly Dictionary<string, byte[]> Signatures = new()
    {
        [".pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 },                               // %PDF
        [".docx"] = new byte[] { 0x50, 0x4B, 0x03, 0x04 },                              // ZIP (PK)
        [".doc"] = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },       // eski Office
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
    };

    public FileValidationResult Validate(string fileName, long length, Stream content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!Signatures.TryGetValue(extension, out var signature))
            return FileValidationResult.Fail(
                $"'{extension}' uzantılı dosyalar kabul edilmez. Kabul edilen formatlar: {AcceptedFormats}");

        if (length <= 0)
            return FileValidationResult.Fail("Dosya boş.");

        if (length > MaxFileSizeBytes)
            return FileValidationResult.Fail("Dosya boyutu en fazla 10 MB olabilir.");

        var header = new byte[signature.Length];
        var read = content.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        content.Position = 0; // sonraki okuma baştan başlasın

        if (read < signature.Length || !header.AsSpan().SequenceEqual(signature.AsSpan()))
            return FileValidationResult.Fail(
                $"Dosyanın içeriği '{extension}' uzantısıyla uyuşmuyor. Dosya bozuk ya da farklı bir türde olabilir.");

        // Kayıtlarda tek biçim: .jpg → .jpeg
        return FileValidationResult.Ok(extension == ".jpg" ? ".jpeg" : extension);
    }
}