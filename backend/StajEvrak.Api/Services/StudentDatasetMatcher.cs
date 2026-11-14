using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Services;

/// <summary>
/// Giriş yapan öğrencinin test veri setindeki kaydını bulur (hoca 4.3:
/// "yüklenen Ek-16'daki öğrenci bilgileri ile giriş yapan kullanıcının veri tabanındaki
/// bilgileri karşılaştırılır").
///
/// Eşleşme e-posta ile yapılır: Hocanın tanımladığı alanlardan hem kullanıcı hesabında
/// (4.1: "kullanıcı adı / e-posta + şifre") hem de veri setinde (4.3: "e-posta") bulunan
/// tek alan e-postadır. Bu yüzden kayıt formuna yeni bir alan eklenmez.
///
/// Kayıt bulunamazsa (öğrenci veri setindeki e-postadan farklı bir e-postayla kayıt olduysa)
/// null döner; karşılaştırmayı yapan servis (2. geliştirici) bunu raporda belirtir.
/// </summary>
public class StudentDatasetMatcher
{
    private readonly AppDbContext _db;

    public StudentDatasetMatcher(AppDbContext db) => _db = db;

    public async Task<AdminTestDataset?> FindForUserAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return null;

        // Kullanıcı e-postaları kayıtta küçük harfe çevrilir; veri setindeki e-posta
        // farklı yazılmış olabileceği için karşılaştırma büyük/küçük harf ve boşluk duyarsızdır.
        var email = user.Email.Trim().ToLowerInvariant();
        return await _db.AdminTestDataset
            .FirstOrDefaultAsync(d => d.Email.Trim().ToLower() == email);
    }
}
