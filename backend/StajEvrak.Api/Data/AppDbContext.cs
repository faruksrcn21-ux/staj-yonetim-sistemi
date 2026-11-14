using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StajEvrak.Api.Models;

namespace StajEvrak.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<BaseUser> Users => Set<BaseUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();
    public DbSet<AdminTestDataset> AdminTestDataset => Set<AdminTestDataset>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Student ve Admin aynı users tablosunda; hangisi olduğunu role sütunu belirler.
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

            e.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);

            // IP kilidi son 15 dakikadaki denemeleri bu iki sütunla sorgular.
            e.HasIndex(l => new { l.IpAddress, l.AttemptTime });
        });

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

            // İçe aktarmada öğrenci kimlik no ile bulunur; aynı kimlik no iki kez olamaz.
            e.HasIndex(d => d.TcNo).IsUnique();
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.ToTable("documents");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasColumnName("id");
            e.Property(d => d.UserId).HasColumnName("user_id");
            e.Property(d => d.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
            e.Property(d => d.FilePath).HasColumnName("file_path").HasMaxLength(500).IsRequired();
            e.Property(d => d.FileExtension).HasColumnName("file_extension").HasMaxLength(10).IsRequired();
            e.Property(d => d.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            e.Property(d => d.ParsedData).HasColumnName("parsed_data");
            e.Property(d => d.ManualFormData).HasColumnName("manual_form_data");
            e.Property(d => d.IsReadyForSubmission).HasColumnName("is_ready_for_submission");
            e.Property(d => d.ComparisonReport).HasColumnName("comparison_report");
            e.Property(d => d.UploadedAt).HasColumnName("uploaded_at");

            e.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(d => d.Status);
        });

        // SQLite saat dilimi saklamaz; okunan tarihler UTC olarak işaretlenir.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
        {
            if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                property.SetValueConverter(utcConverter);
        }
    }
}
