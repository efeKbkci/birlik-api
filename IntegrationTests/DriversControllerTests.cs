using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Birlik.Shared.DTOs;
using Xunit;

namespace IntegrationTests;

public class DriversControllerTests(InMemoryWebApplicationFactory factory) : IntegrationTestBase(factory), IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task DriverFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK HAZIRLIĞI (Test İzolasyonu) ---
        // Sürücüyü ekleyebilmek için önce ona bir Şirket oluşturuyoruz.
        var companyDto = TestDataFactory.CreateNewCompanyObject();
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<DetailedCompanyReadDto>();
        var companyId = createdCompany!.Id;

        // --- 1. ARRANGE: Sürücü Hazırlığı ---
        var driverDto = TestDataFactory.CreateNewDriverObject(companyId);

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/drivers", driverDto, _jsonOptions);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdDriver = await postResponse.Content.ReadFromJsonAsync<DetailedDriverReadDto>(_jsonOptions);
        Assert.NotNull(createdDriver);
        var driverId = createdDriver.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedDriver = await getResponse.Content.ReadFromJsonAsync<DetailedDriverReadDto>(_jsonOptions);
        Assert.Equal(driverDto.FirstName, fetchedDriver?.FirstName);

        // --- 4. UPDATE (PATCH) ---
        var newPhone = "555-999-8877";
        var patchDto = new DriverPatchDto { PhoneNumber = newPhone };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/drivers/{driverId}", patchDto, _jsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        // --- 5. DELETE (Yumuşak Silme) ---
        var deleteResponse = await _client.DeleteAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Standart GET 404 dönmeli
        var verifyDeleteResponse = await _client.GetAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);

        // Silinenler listesinde (company/{id}/deleted) görünmeli
        var deletedListResponse = await _client.GetAsync($"/api/drivers/company/{companyId}/deleted");
        Assert.Equal(HttpStatusCode.OK, deletedListResponse.StatusCode);
        var deletedDrivers = await deletedListResponse.Content.ReadFromJsonAsync<List<DriverDeleteIncludedDto>>(_jsonOptions);
        Assert.Contains(deletedDrivers!, d => d.Id == driverId && d.IsDeleted);

        // --- 6. RESTORE (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/drivers/{driverId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Kurtarıldığını Doğrulama
        var finalVerifyResponse = await _client.GetAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, finalVerifyResponse.StatusCode);
    }
}