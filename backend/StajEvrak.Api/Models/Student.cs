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
}
