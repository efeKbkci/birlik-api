using Tutorial.Mappings;
using Microsoft.EntityFrameworkCore; // EF Core'un komutlarý için gerekli
using Microsoft.Extensions.DependencyInjection;
using Tutorial.Context; // Kendi yazdýðýmýz DbContext'in yolu

// ==========================================
// BÖLÜM 1: UYGULAMA KURULUMU VE SERVÝSLER
// ==========================================
var builder = WebApplication.CreateBuilder(args);

// 1. Controller'larý sisteme tanýtýyoruz (API uç noktalarýmýz için þart)
builder.Services.AddControllers();

// 2. Swagger'ý (API dökümantasyon/test aracý) sisteme tanýtýyoruz
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Veritabaný baðlantýmýzý (DbContext) sisteme dahil ediyoruz (Dependency Injection)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention()); // ÝÞTE BU SÝHÝRLÝ SATIRI EKLEDÝK

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddAutoMapper(cfg => { }, typeof(CompanyMappingProfile), typeof(DriversMappingProfile));

// ==========================================
// BÖLÜM 2: UYGULAMA ÇALIÞMA ZAMANI (PIPELINE)
// ==========================================
var app = builder.Build();

// 4. Eðer geliþtirme ortamýndaysak (Production/Canlý deðilsek) Swagger'ý aktif et
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 5. HTTP üzerinden gelen istekleri güvenli HTTPS protokolüne yönlendir
app.UseHttpsRedirection();

// 6. Gelen isteklerin yetkisi var mý diye kontrol et (Þu an yetki sistemimiz yok ama altyapý dursun)
app.UseAuthorization();

// 7. Gelen istekleri, yazdýðýmýz Controller sýnýflarýna (Örn: TestController) yönlendir
app.MapControllers();

// 8. Motoru çalýþtýr!
app.Run();