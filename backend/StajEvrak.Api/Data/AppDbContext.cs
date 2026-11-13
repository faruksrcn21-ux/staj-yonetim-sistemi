using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Data;

/// <summary>
/// Veritabanı bağlantısı. C# sınıflarını tablolara eşler.
/// Tablo ve sütun isimleri ortak ER diyagramıyla (docs/01_VERITABANI_ER_DIYAGRAMI.md)
/// aynı olacak şekilde snake_case verilmiştir.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BaseUser> Users => Set<BaseUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<AdminTestDataset> AdminTestDataset => Set<AdminTestDataset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---- users ----
        // Student ve Admin aynı "users" tablosunda tutulur (Table-Per-Hierarchy).
        // Hangi satırın Student, hangisinin Admin olduğunu "role" sütunu belirler.
        modelBuilder.Entity<BaseUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            e.Property(u => u.Role).HasColumnName("role").HasMaxLength(20).IsRequired();
            e.Property(u => u.FailedAttempts).HasColumnName("failed_attempts").HasDefaultValue(0);
            e.Property(u => u.LockedUntil).HasColumnName("locked_until");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");

            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();

            e.HasDiscriminator(u => u.Role)
                .HasValue<Student>(Roles.Student)
                .HasValue<Admin>(Roles.Admin);
        });

        // ---- login_logs ----
        modelBuilder.Entity<LoginLog>(e =>
        {
            e.ToTable("login_logs");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.UserId).HasColumnName("user_id");
            e.Property(l => l.AttemptedIdentifier).HasColumnName("attempted_identifier").HasMaxLength(150).IsRequired();
            e.Property(l => l.IpAddress).HasColumnName("ip_address").HasMaxLength(45).IsRequired();
            e.Property(l => l.AttemptTime).HasColumnName("attempt_time");
            e.Property(l => l.LogStatus).HasColumnName("log_status").HasMaxLength(50).IsRequired();

            // Kullanıcı silinirse logları kalır, sadece user_id boşalır.
            e.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);

            // IP kilidi kontrolü "bu IP'den son 15 dk'daki denemeler" diye sorgulayacağı için.
            e.HasIndex(l => new { l.IpAddress, l.AttemptTime });
        });

        // ---- revoked_tokens ----
        modelBuilder.Entity<RevokedToken>(e =>
        {
            e.ToTable("revoked_tokens");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.Jti).HasColumnName("jti").HasMaxLength(64).IsRequired();
            e.Property(t => t.UserId).HasColumnName("user_id");
            e.Property(t => t.ExpiresAt).HasColumnName("expires_at");
            e.Property(t => t.RevokedAt).HasColumnName("revoked_at");

            e.HasIndex(t => t.Jti).IsUnique();
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- admin_test_dataset ----
        modelBuilder.Entity<AdminTestDataset>(e =>
        {
            e.ToTable("admin_test_dataset");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id");
            e.Property(d => d.FullName).HasColumnName("full_name").HasMaxLength(150);
            e.Property(d => d.TcNo).HasColumnName("tc_no").HasMaxLength(11);
            e.Property(d => d.Nationality).HasColumnName("nationality").HasMaxLength(50);
            e.Property(d => d.StudentNo).HasColumnName("student_no").HasMaxLength(20);
            e.Property(d => d.BirthDate).HasColumnName("birth_date");
            e.Property(d => d.Address).HasColumnName("address");
            e.Property(d => d.Department).HasColumnName("department").HasMaxLength(100);
            e.Property(d => d.SubmissionYear).HasColumnName("submission_year").HasMaxLength(20);
            e.Property(d => d.PreviousInternshipDays).HasColumnName("previous_internship_days");
            e.Property(d => d.Email).HasColumnName("email").HasMaxLength(150);
            e.Property(d => d.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(d => d.HealthInsuranceStatus).HasColumnName("health_insurance_status").HasMaxLength(50);
        });
    }
}
