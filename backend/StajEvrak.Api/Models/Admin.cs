namespace StajEvrak.Api.Models;

// Kalıtım: BaseUser'dan türer. Admin hesabı kayıt ekranından değil, açılışta seed ile oluşur.
public class Admin : BaseUser
{
    private Admin() { }

    public Admin(string username, string email, string passwordHash)
        : base(username, email, passwordHash, Roles.Admin)
    {
    }

    public override string GetDashboardView() => "/admin";

    // Admin her belgeye erişir; Tamamlandı belgeyi de güncelleyip silebilir.
    public override DocumentAccess CheckDocumentAccess(Document document, bool modify)
        => DocumentAccess.Allowed;
}
