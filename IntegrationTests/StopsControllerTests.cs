using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class StopsControllerTests(InMemoryWebApplicationFactory factory) : IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task StopFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAÐIMLILIK HAZIRLIÐI (Test Ýzolasyonu) ---
        // Önce bir þirket ve iki þehir oluþturuyoruz, sonra bir rota oluþturacaðýz.
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", TestDataFactory.CreateNewCompanyObject());
        var company = await companyResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        var companyId = company!.Id;

        var city1Response = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city1 = await city1Response.Content.ReadFromJsonAsync<CityReadDto>();
        var departureId = city1!.Id;

        var city2Response = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city2 = await city2Response.Content.ReadFromJsonAsync<CityReadDto>();
        var arrivalId = city2!.Id;

        // Rota oluþtur
        var routeDto = TestDataFactory.CreateNewRouteObject(departureId, arrivalId);
        var routeResponse = await _client.PostAsJsonAsync("/api/routes", routeDto);
        var createdRoute = await routeResponse.Content.ReadFromJsonAsync<RouteReadDto>();
        var routeId = createdRoute!.Id;

        // --- 1. ARRANGE: Durak Hazýrlýðý ---
        var stopCreate = TestDataFactory.CreateNewStopObject(companyId, routeId);

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/stops", stopCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdStop = await postResponse.Content.ReadFromJsonAsync<StopReadDto>();
        Assert.NotNull(createdStop);
        var stopId = createdStop.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/stops/{stopId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<StopReadDto>();
        Assert.Equal(stopCreate.StopName, fetched?.StopName);
        Assert.Equal(stopCreate.StopOrder, fetched?.StopOrder);

        // Query ile listeleme
        var listResponse = await _client.GetAsync($"/api/stops?companyId={companyId}&routeId={routeId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<StopReadDto>>();
        Assert.Contains(list!, s => s.Id == stopId);

        // --- 4. UPDATE (PATCH) ---
        var patchDto = new StopPatchDto { StopName = "Renamed Stop", StopOrder = 2 };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/stops/{stopId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var getAfterPatch = await _client.GetAsync($"/api/stops/{stopId}");
        var patched = await getAfterPatch.Content.ReadFromJsonAsync<StopReadDto>();
        Assert.Equal("Renamed Stop", patched?.StopName);
        Assert.Equal(2, patched?.StopOrder);

        // --- 5. DELETE (Yumuþak Silme) ---
        var deleteResponse = await _client.DeleteAsync($"/api/stops/{stopId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var verifyDelete = await _client.GetAsync($"/api/stops/{stopId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDelete.StatusCode);

        var deletedListResponse = await _client.GetAsync($"/api/stops/company/{companyId}/deleted");
        Assert.Equal(HttpStatusCode.OK, deletedListResponse.StatusCode);
        var deletedList = await deletedListResponse.Content.ReadFromJsonAsync<List<StopDeleteIncludedDto>>();
        Assert.Contains(deletedList!, s => s.Id == stopId);

        // --- 6. RESTORE (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/stops/{stopId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var finalGet = await _client.GetAsync($"/api/stops/{stopId}");
        Assert.Equal(HttpStatusCode.OK, finalGet.StatusCode);
    }
}
