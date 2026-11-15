using StajEvrak.Api.Comparison;
using StajEvrak.Api.Parsing;
using StajEvrak.Api.Repositories;
using StajEvrak.Api.Services;
using StajEvrak.Api.Storage;

namespace StajEvrak.Api.Extensions;

public static class DocumentModuleExtensions
{
    public static IServiceCollection AddDocumentUploadModule(this IServiceCollection services)
    {
        services.AddScoped<IDocumentParser, PdfDocumentParser>();
        services.AddScoped<IDocumentParser, DocxDocumentParser>();
        services.AddScoped<IDocumentParser, ImageDocumentParser>();
        services.AddScoped<IDocumentParser, LegacyWordDocumentParser>();
        services.AddScoped<DocumentParserFactory>();

        services.AddScoped<Ek16FieldExtractor>();
        services.AddScoped<DocumentComparisonService>();

        services.AddScoped<UploadedFileValidator>();
        services.AddSingleton<IDocumentFileStore, LocalDocumentFileStore>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<DocumentUploadService>();
        return services;
    }
}