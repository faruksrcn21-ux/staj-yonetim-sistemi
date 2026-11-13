// Staj Evrak Doğrulama ve Yönetim Sistemi - Backend giriş noktası.
// Uygulama burada ayağa kalkar: servisler kaydedilir, ardından
// HTTP istek hattı (middleware pipeline) kurulur.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StajEvrak.Api.Data;
using StajEvrak.Api.Dtos;
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
builder.Services.AddScoped<JwtTokenService>();

// ---- JWT ayarları ----
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key ayarı eksik veya 32 karakterden kısa. appsettings.Development.json veya Jwt__Key ortam değişkenini kontrol edin.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Claim isimlerini olduğu gibi kullan ("role" -> "role"), .NET'in uzun isimlerine çevirme.
        options.MapInboundClaims = false;

        // Her istekte token şu kurallara göre doğrulanır:
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,                 // süresi dolmuş token reddedilir
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,         // imzası bozuk (değiştirilmiş) token reddedilir
            IssuerSigningKey = JwtTokenService.CreateSigningKey(jwt.Key),
            NameClaimType = JwtTokenService.NameClaim,
            RoleClaimType = JwtTokenService.RoleClaim, // [Authorize(Roles=...)] bu claim'e bakar
        };

        // Hoca: "Yetkisiz istekler 403 döner ve login sayfasına yönlendirilir."
        // Bu yüzden hem giriş yapmamış hem de rolü yetmeyen isteklere 403 döner.
        // API sayfa yönlendirmesi yapamaz; bunun yerine yanıtta redirectTo="/login" döner,
        // frontend bunu görünce kullanıcıyı login sayfasına yönlendirir.
        options.Events = new JwtBearerEvents
        {
            // İmza ve süre geçerli olsa bile, çıkış (logout) yapılmış token'ı reddet.
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var auth = context.HttpContext.RequestServices.GetRequiredService<AuthService>();

                if (jti is null || await auth.IsTokenRevokedAsync(jti))
                    context.Fail("Token çıkış yapılarak iptal edilmiş.");
            },
            // Token yok / geçersiz / süresi dolmuş / iptal edilmiş -> 403
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ErrorResponse(
                    "Bu işlem için giriş yapmanız gerekiyor.", RedirectTo: "/login"));
            },
            // Token geçerli ama rol yetmiyor -> 403
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ErrorResponse(
                    "Bu sayfaya erişim yetkiniz yok.", RedirectTo: "/login"));
            },
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Uygulama açılırken:
//  1) Bekleyen migration'lar uygulanır (ekipte herkes "dotnet run" dediğinde tablolar oluşur).
//  2) Hiç admin yoksa appsettings'teki "AdminSeed" bilgileriyle admin hesabı oluşturulur.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var adminSeed = builder.Configuration.GetSection(AdminSeedOptions.SectionName).Get<AdminSeedOptions>()
                    ?? new AdminSeedOptions();
    await DbSeeder.SeedAdminAsync(
        db,
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>(),
        adminSeed,
        app.Logger);
}

// ---- HTTP istek hattı ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // 1) Token'ı oku ve doğrula: "Bu kim?"
app.UseAuthorization();  // 2) Rolü kontrol et: "Buna yetkisi var mı?"
app.MapControllers();

app.Run();
