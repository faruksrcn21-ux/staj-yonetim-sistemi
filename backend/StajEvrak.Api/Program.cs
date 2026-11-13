// Staj Evrak Doğrulama ve Yönetim Sistemi - Backend giriş noktası.
// Uygulama burada ayağa kalkar: servisler kaydedilir, ardından
// HTTP istek hattı (middleware pipeline) kurulur.

using Microsoft.EntityFrameworkCore;
using StajEvrak.Api.Data;
using StajEvrak.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Servis kayıtları ----
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // /openapi/v1.json (sadece Development'ta açılır)

// Veritabanı: SQLite (bağlantı bilgisi appsettings.json -> ConnectionStrings:Default)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Auth servisleri (her HTTP isteği için yeni bir örnek oluşturulur)
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Uygulama açılırken bekleyen migration'ları uygular. Böylece ekipte herkes
// "dotnet run" dediğinde veritabanı tabloları otomatik oluşur.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ---- HTTP istek hattı ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
