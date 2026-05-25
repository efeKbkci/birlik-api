using IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tutorial.Context;

public abstract class IntegrationTestBase : IClassFixture<InMemoryWebApplicationFactory>
{
    protected readonly HttpClient _client;
    protected readonly AppDbContext _context;

    // İşte aradığın o merkezi yapılandırma!
    protected readonly JsonSerializerOptions _jsonOptions;

    protected IntegrationTestBase(InMemoryWebApplicationFactory factory)
    {
        _client = factory.CreateClient();

        // Veritabanı bağlamını (DbContext) testlerde kullanmak üzere al
        var scope = factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // İstemcinin (HttpClient) JSON okuma ayarlarını MERKEZİLEŞTİR
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}