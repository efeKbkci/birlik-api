using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class DriversControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DriversControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DriverFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK HAZIRLIĞI (Test İzolasyonu) ---
        // Sürücüyü ekleyebilmek için önce ona bir Şirket oluşturuyoruz.
        var companyDto = new CompanyCreateDto { CompanyName = "DriverTestCorp", ContactPhone = "123", Location = "Bursa", IsActive = true };
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        var companyId = createdCompany!.Id;

        // --- 1. ARRANGE: Sürücü Hazırlığı ---
        var randomFirstName = "Driver_" + Guid.NewGuid().ToString()[..5];
        var createDto = new DriverCreateDto
        {
            CompanyId = companyId,
            FirstName = randomFirstName,
            LastName = "Yılmaz",
            PhoneNumber = "555-123-4567",
            PasswordHash = "TestHash123",
            IsActive = true
        };

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/drivers", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdDriver = await postResponse.Content.ReadFromJsonAsync<DriverReadDto>();
        Assert.NotNull(createdDriver);
        var driverId = createdDriver.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedDriver = await getResponse.Content.ReadFromJsonAsync<DriverReadDto>();
        Assert.Equal(randomFirstName, fetchedDriver?.FirstName);

        // --- 4. UPDATE (PATCH) ---
        var newPhone = "555-999-8877";
        var patchDto = new DriverPatchDto { PhoneNumber = newPhone };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/drivers/{driverId}", patchDto);
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
        var deletedDrivers = await deletedListResponse.Content.ReadFromJsonAsync<List<DriverDeleteIncludedDto>>();
        Assert.Contains(deletedDrivers!, d => d.Id == driverId && d.IsDeleted);

        // --- 6. RESTORE (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/drivers/{driverId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Kurtarıldığını Doğrulama
        var finalVerifyResponse = await _client.GetAsync($"/api/drivers/{driverId}");
        Assert.Equal(HttpStatusCode.OK, finalVerifyResponse.StatusCode);
    }
}