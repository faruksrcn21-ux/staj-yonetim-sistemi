using StajEvrak.Api.Readers;
using StajEvrak.Api.Repositories;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Extensions;

//
// Veri seti modülünün tüm servis kayıtları tek yerde. Program.cs'e sadece tek satır eklenir;
// böylece ortak dosyada birleştirme çakışması ihtimali en aza iner.
// 
public static class DatasetModuleExtensions
{
    public static IServiceCollection AddDatasetModule(this IServiceCollection services)
    {
        services.AddScoped<IStudentDatasetReader, CsvStudentDatasetReader>();
        services.AddScoped<IStudentDatasetReader, ExcelStudentDatasetReader>();
        services.AddScoped<DatasetReaderFactory>();
        services.AddScoped<ITestDatasetRepository, TestDatasetRepository>();
        services.AddScoped<IDatasetImportService, DatasetImportService>();
        return services;
    }
}