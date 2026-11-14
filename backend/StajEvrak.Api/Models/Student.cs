namespace StajEvrak.Api.Models;

// Kalıtım: BaseUser'dan türer.
public class Student : BaseUser
{
    private Student() { }

    public Student(string username, string email, string passwordHash)
        : base(username, email, passwordHash, Roles.Student)
    {
    }

    public override string GetDashboardView() => "/student";

    // Öğrenci sadece kendi belgesine erişir, Tamamlandı belgeyi değiştiremez/silemez.
    public override DocumentAccess CheckDocumentAccess(Document document, bool modify)
    {
        if (document.UserId != Id)
            return DocumentAccess.NotOwner;

        if (modify && document.IsCompleted)
            return DocumentAccess.Completed;

        return DocumentAccess.Allowed;
    }
}
