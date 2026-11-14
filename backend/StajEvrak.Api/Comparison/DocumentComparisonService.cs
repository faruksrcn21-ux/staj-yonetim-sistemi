using System.Text.Json;
using StajEvrak.Api.Models;
using StajEvrak.Api.Parsing;
using StajEvrak.Api.Services;

namespace StajEvrak.Api.Comparison;

public record FieldMismatch(string Field, string Label, string? Entered, string? Found, string Source);

public record ComparisonReport(string Note, IReadOnlyList<FieldMismatch> Mismatches);

// Öğrencinin forma girdiği bilgileri iki kaynakla karşılaştırır:
//   1) belgeden okunan bilgiler, 2) test veri setindeki kaydı (e-postayla bulunur).
// Uyuşmazlık yoksa belge teslime hazırdır ve admin onayını bekler; varsa Hatalı/Eksik olur.
public class DocumentComparisonService
{
    public const string SourceDocument = "belge";
    public const string SourceDataset = "veri seti";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly Ek16FieldExtractor _extractor;
    private readonly StudentDatasetMatcher _matcher;

    public DocumentComparisonService(Ek16FieldExtractor extractor, StudentDatasetMatcher matcher)
    {
        _extractor = extractor;
        _matcher = matcher;
    }

    // Belge Yüklendi → İnceleniyor olur, sonuç kaydedilir (gerekirse Hatalı/Eksik).
    public async Task CompareAndRecordAsync(Document document, ParsedDocument parsed)
    {
        var entered = ReadManualData(document.ManualFormData);
        var fromDocument = _extractor.Extract(parsed);
        var record = await _matcher.FindForUserAsync(document.UserId);
        var fromDataset = record is null ? null : Ek16Fields.FromDataset(record);

        var mismatches = new List<FieldMismatch>();
        foreach (var field in Ek16Fields.All)
        {
            entered.TryGetValue(field.Key, out var value);

            if (fromDocument.TryGetValue(field.Key, out var docValue) && !field.Matches(value, docValue))
                mismatches.Add(new(field.Key, field.Label, value, docValue, SourceDocument));

            if (fromDataset is not null && !field.Matches(value, fromDataset[field.Key]))
                mismatches.Add(new(field.Key, field.Label, value, fromDataset[field.Key], SourceDataset));
        }

        var isReady = fromDataset is not null && mismatches.Count == 0;
        var report = new ComparisonReport(BuildNote(parsed, fromDocument.Count, fromDataset is not null), mismatches);

        document.StartReview();
        document.RecordComparisonResult(
            JsonSerializer.Serialize(fromDocument, Json), isReady, JsonSerializer.Serialize(report, Json));
    }

    private static string BuildNote(ParsedDocument parsed, int fieldsFound, bool datasetFound)
    {
        var notes = new List<string>();

        if (!datasetFound)
            notes.Add("Test veri setinde e-posta adresinizle eşleşen kayıt bulunamadı.");

        if (!parsed.IsReadable)
            notes.Add($"{parsed.UnreadableReason} Girilen bilgiler sadece veri setiyle karşılaştırıldı; belgeyi admin inceleyecek.");
        else if (fieldsFound == 0)
            notes.Add("Belgede tanınan bir alan bulunamadı; girilen bilgiler sadece veri setiyle karşılaştırıldı.");
        else
            notes.Add($"Belgeden {fieldsFound} alan okundu ve karşılaştırıldı.");

        return string.Join(" ", notes);
    }

    private static Dictionary<string, string> ReadManualData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!
                .ToDictionary(kv => kv.Key, kv => kv.Value.ValueKind == JsonValueKind.String
                    ? kv.Value.GetString() ?? string.Empty
                    : kv.Value.ToString());
        }
        catch (JsonException)
        {
            return new();
        }
    }
}
