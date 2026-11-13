// Staj Evrak Doğrulama ve Yönetim Sistemi - Backend giriş noktası.
// Uygulama burada ayağa kalkar: servisler kaydedilir, ardından
// HTTP istek hattı (middleware pipeline) kurulur.

var builder = WebApplication.CreateBuilder(args);

// ---- Servis kayıtları ----
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // /openapi/v1.json (sadece Development'ta açılır)

var app = builder.Build();

// ---- HTTP istek hattı ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
