namespace StajEvrak.Api.Models;

/// <summary>
/// Sistemdeki roller. Hocanın dokümanındaki isimlerle birebir aynıdır.
/// Bu değerler hem veritabanındaki "role" sütununa hem de JWT içindeki
/// rol bilgisine yazılır. [Authorize(Roles = Roles.Admin)] gibi kullanılır.
/// </summary>
public static class Roles
{
    public const string Student = "ROLE_STUDENT";
    public const string Admin = "ROLE_ADMIN";
}
