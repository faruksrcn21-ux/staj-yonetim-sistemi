using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StajEvrak.Api.Data;
using StajEvrak.Api.Dtos;
using StajEvrak.Api.Services;
using StajEvrak.Api.Extensions;

// Gizli ayarlar (JWT anahtarı, admin şifresi) .env dosyasından okunur.
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<StudentDatasetMatcher>();
builder.Services.AddDatasetModule();
builder.Services.AddDocumentUploadModule();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwt = jwtSection.Get<JwtOptions>() ?? new JwtOptions();

if (jwt.Key.Length < 32)
    throw new InvalidOperationException(
        "Jwt__Key ayarı eksik veya 32 karakterden kısa. backend/StajEvrak.Api/.env.example dosyasını .env olarak kopyalayıp doldurun.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtTokenService.CreateSigningKey(jwt.Key),
            NameClaimType = JwtTokenService.NameClaim,
            RoleClaimType = JwtTokenService.RoleClaim,
        };

        // Giriş yapmamış ve rolü yetmeyen isteklere 403 + redirectTo="/login" döner;
        // yönlendirmeyi frontend yapar.
        options.Events = new JwtBearerEvents
        {
            // Çıkış yapılmış token geçerli görünse bile reddedilir.
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var auth = context.HttpContext.RequestServices.GetRequiredService<AuthService>();

                if (jti is null || await auth.IsTokenRevokedAsync(jti))
                    context.Fail("Token çıkış yapılarak iptal edilmiş.");
            },
            // Token yok, geçersiz, süresi dolmuş veya iptal edilmiş
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ErrorResponse(
                    "Bu işlem için giriş yapmanız gerekiyor.", RedirectTo: "/login"));
            },
            // Token geçerli ama rol yetmiyor
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new ErrorResponse(
                    "Bu sayfaya erişim yetkiniz yok.", RedirectTo: "/login"));
            },
        };
    });

builder.Services.AddAuthorization();

// Sadece bu bilgisayardaki adreslerden (localhost) gelen isteklere izin verilir.
// Dosyadan açılan HTML "null" adres gönderir; buna sadece geliştirmede izin verilir.
const string FrontendCorsPolicy = "Frontend";
var allowNullOrigin = builder.Environment.IsDevelopment();

builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy, policy => policy
    .SetIsOriginAllowed(origin =>
        origin == "null"
            ? allowNullOrigin
            : Uri.TryCreate(origin, UriKind.Absolute, out var uri)
              && (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

// Açılışta tablolar oluşturulur/güncellenir ve admin yoksa oluşturulur.
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication(); // Token'ı doğrula: bu kim?
app.UseAuthorization();  // Rolü kontrol et: yetkisi var mı?
app.MapControllers();

app.Run();
