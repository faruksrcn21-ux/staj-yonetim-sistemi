using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

// Öğrencinin veri setindeki kaydını bulur. Hem hesapta hem veri setinde olan tek alan
// e-posta olduğu için eşleşme e-postayla yapılır; bulunamazsa null döner.
public class StudentDatasetMatcher
{
    private readonly AppDbContext _db;

    public StudentDatasetMatcher(AppDbContext db) => _db = db;

    public async Task<AdminTestDataset?> FindForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return null;

        // Büyük/küçük harf ve boşluk farkı önemsenmez.
        var email = user.Email.Trim().ToLowerInvariant();
        return await _db.AdminTestDataset
            .FirstOrDefaultAsync(d => d.Email.Trim().ToLower() == email);
    }
}
