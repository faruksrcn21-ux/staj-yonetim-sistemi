namespace StajEvrak.Api.Models;

/// <summary>
/// Öğrenci kullanıcı (Kalıtım: BaseUser'dan türer).
/// Öğrenciler hesabını kendisi oluşturur (hoca cevabı #4).
/// </summary>
public class Student : BaseUser
{
    // EF Core için.
    private Student() { }

    public Student(string username, string email, string passwordHash)
        : base(username, email, passwordHash, Roles.Student)
    {
    }

    public override string GetDashboardView() => "/student";

    /// <summary>
    /// Öğrenci sadece kendi belgesine erişir. "Tamamlandı" statüsündeki belgeyi
    /// hiçbir şekilde değiştiremez/silemez (hoca S3), sadece görüntüleyebilir.
    /// </summary>
    public override DocumentAccess CheckDocumentAccess(Document document, bool modify)
    {
        if (document.UserId != Id)
            return DocumentAccess.NotOwner;

        if (modify && document.IsCompleted)
            return DocumentAccess.Completed;

        return DocumentAccess.Allowed;
    }
}
