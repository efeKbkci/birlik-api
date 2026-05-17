using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class VehiclesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public VehiclesControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task VehicleFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK HAZIRLIĞI (Test İzolasyonu) ---
        // Araç oluşturabilmek için önce bir Şirket ve bir Sürücü oluşturuyoruz.
        var companyDto = new CompanyCreateDto { CompanyName = "VehicleTestCorp", ContactPhone = "321", Location = "İzmir", IsActive = true };
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        var companyId = createdCompany!.Id;

        var driverCreate = new DriverCreateDto
        {
            CompanyId = companyId,
            FirstName = "VehDrv_" + Guid.NewGuid().ToString()[..5],
            LastName = "Test",
            PhoneNumber = "555-000-1111",
            PasswordHash = "Hash",
            IsActive = true
        };

        var driverResponse = await _client.PostAsJsonAsync("/api/drivers", driverCreate);
        var createdDriver = await driverResponse.Content.ReadFromJsonAsync<DriverReadDto>();
        var driverId = createdDriver!.Id;

        // --- 1. ARRANGE: Araç Hazırlığı ---
        var plate = "TR-" + Guid.NewGuid().ToString()[..6];
        var createDto = new VehicleCreateDto
        {
            CompanyId = companyId,
            DefaultDriverId = driverId,
            PlateNumber = plate,
            Capacity = 20,
            IsActive = true
        };

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/vehicles", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdVehicle = await postResponse.Content.ReadFromJsonAsync<VehicleReadDto>();
        Assert.NotNull(createdVehicle);
        var vehicleId = createdVehicle.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedVehicle = await getResponse.Content.ReadFromJsonAsync<VehicleReadDto>();
        Assert.Equal(plate, fetchedVehicle?.PlateNumber);
        Assert.Equal(20, fetchedVehicle?.Capacity);

        // --- 4. UPDATE (PATCH) ---
        var newPlate = "TR-NEW-" + Guid.NewGuid().ToString()[..4];
        var patchDto = new VehiclePatchDto { PlateNumber = newPlate };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/vehicles/{vehicleId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        // Doğrula
        var getAfterPatch = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        var fetchedAfterPatch = await getAfterPatch.Content.ReadFromJsonAsync<VehicleReadDto>();
        Assert.Equal(newPlate, fetchedAfterPatch?.PlateNumber);

        // --- 5. DELETE (Yumuşak Silme) ---
        var deleteResponse = await _client.DeleteAsync($"/api/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Standart GET 404 dönmeli
        var verifyDeleteResponse = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);

        // Silinenler listesinde (company/{id}/deleted) görünmeli
        var deletedListResponse = await _client.GetAsync($"/api/vehicles/company/{companyId}/deleted");
        Assert.Equal(HttpStatusCode.OK, deletedListResponse.StatusCode);
        var deletedVehicles = await deletedListResponse.Content.ReadFromJsonAsync<List<VehicleDeleteIncludedDto>>();
        Assert.Contains(deletedVehicles!, v => v.Id == vehicleId);

        // --- 6. RESTORE (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/vehicles/{vehicleId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Kurtarıldığını Doğrulama
        var finalVerifyResponse = await _client.GetAsync($"/api/vehicles/{vehicleId}");
        Assert.Equal(HttpStatusCode.OK, finalVerifyResponse.StatusCode);
    }
}
