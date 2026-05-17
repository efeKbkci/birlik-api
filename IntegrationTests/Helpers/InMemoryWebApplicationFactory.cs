using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Tutorial.Context; // AppDbContext'in bulunduğu yer

namespace IntegrationTests.Helpers;

// Test verilerinin, gerçek veri tabanını kirletmemesi için bellekte geçici bir veri tabanı oluşturuyoruz. 
public class InMemoryWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // 1. Orijinal API'daki AppDbContext (gerçek veritabanı) ayarını bul
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            // 2. Eğer bulduysan, bu servisi sistemden kaldır
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // 3. Yerine In-Memory Database (RAM Veritabanı) ekle
            // Her test çalıştığında tertemiz, taze bir veritabanı oluşur
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("TutorialTestDb");
            });
        });
    }
}
