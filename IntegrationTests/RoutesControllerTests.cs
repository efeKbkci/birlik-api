using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class RoutesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RoutesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RouteFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK HAZIRLIĞI (Test İzolasyonu) ---
        // Güzergah için iki tane bağımsız şehir oluşturuyoruz.
        var city1Response = await _client.PostAsJsonAsync("/api/cities", new CityCreateDto { Name = "Kalkış_" + Guid.NewGuid().ToString()[..5] });
        var city1 = await city1Response.Content.ReadFromJsonAsync<CityReadDto>();

        var city2Response = await _client.PostAsJsonAsync("/api/cities", new CityCreateDto { Name = "Varış_" + Guid.NewGuid().ToString()[..5] });
        var city2 = await city2Response.Content.ReadFromJsonAsync<CityReadDto>();

        // --- 1. ARRANGE: Güzergah Hazırlığı ---
        var randomDuration = new Random().Next(60, 600);
        var createDto = new RouteCreateDto
        {
            DepartureCityId = city1!.Id,
            ArrivalCityId = city2!.Id,
            EstimatedDuration = randomDuration
        };

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/routes", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdRoute = await postResponse.Content.ReadFromJsonAsync<RouteReadDto>();
        Assert.NotNull(createdRoute);
        var routeId = createdRoute.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/routes/{routeId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedRoute = await getResponse.Content.ReadFromJsonAsync<RouteReadDto>();
        Assert.Equal(randomDuration, fetchedRoute?.EstimatedDuration);

        // AutoMapper Şehir isimlerini de getirmiş mi?
        Assert.NotNull(fetchedRoute?.DepartureCityName);
        Assert.NotNull(fetchedRoute?.ArrivalCityName);

        // --- 4. UPDATE (PATCH) ---
        var newDuration = randomDuration + 30;
        var patchDto = new RoutePatchDto { EstimatedDuration = newDuration };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/routes/{routeId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        // --- 5. DELETE (Yumuşak Silme) ---
        var deleteResponse = await _client.DeleteAsync($"/api/routes/{routeId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Standart GET 404 dönmeli
        var verifyDeleteResponse = await _client.GetAsync($"/api/routes/{routeId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);

        // --- 6. RESTORE (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/routes/{routeId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Kurtarıldığını Doğrulama
        var finalVerifyResponse = await _client.GetAsync($"/api/routes/{routeId}");
        Assert.Equal(HttpStatusCode.OK, finalVerifyResponse.StatusCode);
    }
}