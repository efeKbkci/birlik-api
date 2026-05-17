using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class RoutesControllerTests(InMemoryWebApplicationFactory factory) : IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RouteFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK HAZIRLIĞI (Test İzolasyonu) ---
        // Güzergah için iki tane bağımsız şehir oluşturuyoruz.
        var city1Response = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city1 = await city1Response.Content.ReadFromJsonAsync<CityReadDto>();
        var departureId = city1!.Id;

        var city2Response = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city2 = await city2Response.Content.ReadFromJsonAsync<CityReadDto>();
        var arrivalId = city2!.Id;

        // --- 1. ARRANGE: Güzergah Hazırlığı ---
        var routeDto = TestDataFactory.CreateNewRouteObject(departureId, arrivalId);

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/routes", routeDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdRoute = await postResponse.Content.ReadFromJsonAsync<RouteReadDto>();
        Assert.NotNull(createdRoute);
        var routeId = createdRoute.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/routes/{routeId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedRoute = await getResponse.Content.ReadFromJsonAsync<RouteReadDto>();
        Assert.Equal(routeDto.EstimatedDuration, fetchedRoute?.EstimatedDuration);

        // AutoMapper Şehir isimlerini de getirmiş mi?
        Assert.NotNull(fetchedRoute?.DepartureCityName);
        Assert.NotNull(fetchedRoute?.ArrivalCityName);

        // --- 4. UPDATE (PATCH) ---
        var newDuration = routeDto.EstimatedDuration + 30;
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