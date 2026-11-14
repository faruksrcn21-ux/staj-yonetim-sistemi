namespace StajEvrak.Api.Models;

/// <summary>
/// Admin kullanıcı (Kalıtım: BaseUser'dan türer).
/// Kayıt ekranından admin oluşturulamaz; admin hesabı uygulama açılışında
/// seed ile oluşturulur.
/// </summary>
public class Admin : BaseUser
{
    // EF Core için.
    private Admin() { }

    public Admin(string username, string email, string passwordHash)
        : base(username, email, passwordHash, Roles.Admin)
    {
    }

    public override string GetDashboardView() => "/admin";

    /// <summary>
    /// Admin tüm belgeleri görüntüler; "Tamamlandı" dahil her belgeyi güncelleyip
    /// silebilir (hoca S3: "Sadece Admin sayfasından güncelleme ve silme yapılabilir").
    /// </summary>
    public override DocumentAccess CheckDocumentAccess(Document document, bool modify)
        => DocumentAccess.Allowed;
}
