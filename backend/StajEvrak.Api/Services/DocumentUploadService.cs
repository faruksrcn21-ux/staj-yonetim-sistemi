using StajEvrak.Api.Comparison;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Models;
using StajEvrak.Api.Parsing;
using StajEvrak.Api.Repositories;
using StajEvrak.Api.Storage;

namespace StajEvrak.Api.Services;

// Sonuç üç durumdan biri: başarılı, dosya geçersiz veya yetki yok.
public sealed record UploadResult(
    Document? Document, ParseSummary? Parse, string? InvalidFileMessage, DocumentOperationResult? AccessDenied)
{
    public static UploadResult Success(Document d, ParseSummary p) => new(d, p, null, null);
    public static UploadResult Invalid(string message) => new(null, null, message, null);
    public static UploadResult Denied(DocumentOperationResult access) => new(null, null, null, access);
}

// Akış: format kontrolü → dosyayı kaydet → kaydı oluştur → belgeyi oku → karşılaştır.
public class DocumentUploadService
{
    private readonly UploadedFileValidator _validator;
    private readonly IDocumentFileStore _store;
    private readonly IDocumentRepository _repository;
    private readonly DocumentParserFactory _parsers;
    private readonly DocumentService _documents;
    private readonly DocumentComparisonService _comparison;

    public DocumentUploadService(
        UploadedFileValidator validator, IDocumentFileStore store, IDocumentRepository repository,
        DocumentParserFactory parsers, DocumentService documents, DocumentComparisonService comparison)
    {
        _validator = validator;
        _store = store;
        _repository = repository;
        _parsers = parsers;
        _documents = documents;
        _comparison = comparison;
    }

    public async Task<UploadResult> UploadAsync(
        int userId, string fileName, long length, Stream content, string? manualFormData)
    {
        var validation = _validator.Validate(fileName, length, content);
        if (!validation.IsValid)
            return UploadResult.Invalid(validation.Error!);

        var path = await _store.SaveAsync(content, validation.Extension);

        var document = new Document(
            userId, Path.GetFileName(fileName), path, validation.Extension, DateTime.UtcNow, manualFormData);

        try
        {
            await _repository.AddAsync(document);
        }
        catch
        {
            _store.Delete(path); // kayıt oluşmadıysa dosya da kalmasın
            throw;
        }

        return UploadResult.Success(document, await CheckAsync(document));
    }

    // Belge tekrar Yüklendi olur, eski dosya silinir.
    public async Task<UploadResult> ReuploadAsync(
        int documentId, int userId, string fileName, long length, Stream content, string? manualFormData)
    {
        var access = await _documents.CheckAccessAsync(documentId, userId, modify: true);
        if (access.Status != DocumentOperationStatus.Success)
            return UploadResult.Denied(access);

        var validation = _validator.Validate(fileName, length, content);
        if (!validation.IsValid)
            return UploadResult.Invalid(validation.Error!);

        var document = access.Document!;
        var oldPath = document.FilePath;

        var newPath = await _store.SaveAsync(content, validation.Extension);
        document.ReplaceFile(
            Path.GetFileName(fileName), newPath, validation.Extension, DateTime.UtcNow, manualFormData);
        await _repository.SaveChangesAsync();

        _store.Delete(oldPath);
        return UploadResult.Success(document, await CheckAsync(document));
    }

    private async Task<ParseSummary> CheckAsync(Document document)
    {
        // Çok biçimlilik: hangi parser seçilirse seçilsin burada sadece Parse() çağrılır.
        var parsed = _parsers.Select(document.FileExtension).Parse(document.FilePath);

        await _comparison.CompareAndRecordAsync(document, parsed);
        await _repository.SaveChangesAsync();

        return ParseSummary.From(parsed);
    }
}