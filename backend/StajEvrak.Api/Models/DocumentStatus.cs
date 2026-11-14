namespace StajEvrak.Api.Models;

/// <summary>
/// Belge statüleri (documents.status sütunu). Değerler ortak ER diyagramıyla aynıdır.
/// Hocanın akışı (Bölüm 5):
/// Yüklendi → İnceleniyor → Hatalı / Eksik → (düzeltilip yeniden yüklenir) → Onaylandı → Tamamlandı
/// </summary>
public static class DocumentStatus
{
    public const string Yuklendi = "YUKLENDI";
    public const string Inceleniyor = "INCELENIYOR";
    public const string HataliEksik = "HATALI_EKSIK";
    public const string Onaylandi = "ONAYLANDI";
    public const string Tamamlandi = "TAMAMLANDI";
}
