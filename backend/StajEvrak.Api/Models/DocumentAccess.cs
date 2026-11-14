namespace StajEvrak.Api.Models;

/// <summary>
/// Bir kullanıcının bir belgeye erişim kontrolünün sonucu.
/// Bkz. <see cref="BaseUser.CheckDocumentAccess"/>.
/// </summary>
public enum DocumentAccess
{
    /// <summary>İzin var.</summary>
    Allowed,

    /// <summary>Öğrenci başka bir öğrencinin belgesine erişmeye çalışıyor.</summary>
    NotOwner,

    /// <summary>Öğrenci "Tamamlandı" belgesini değiştirmeye/silmeye çalışıyor (hoca S3).</summary>
    Completed,
}
