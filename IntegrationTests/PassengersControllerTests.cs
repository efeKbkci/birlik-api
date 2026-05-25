using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class PassengersControllerTests(InMemoryWebApplicationFactory factory) : IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PassengerFullLifecycle_ShouldWorkCorrectly()
    {
        // --- ARRANGE: Yeni yolcu oluşturmak için TestDataFactory kullan ---
        var createDto = TestDataFactory.CreateNewPassengerObject();

        // --- 1. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/passengers", createDto);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<PassengerReadDto>();
        Assert.NotNull(created);
        var passengerId = created.Id;

        // --- 2. READ (GET) by id ---
        var getResponse = await _client.GetAsync($"/api/passengers/{passengerId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<PassengerReadDto>();
        Assert.Equal(createDto.PhoneNumber, fetched?.PhoneNumber);
        Assert.Equal(createDto.FirstName, fetched?.FirstName);

        // --- 4. UPDATE (PATCH) ---
        var patchDto = new PassengerPatchDto { FirstName = "UpdatedName" };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/passengers/{passengerId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var getAfterPatch = await _client.GetAsync($"/api/passengers/{passengerId}");
        var patched = await getAfterPatch.Content.ReadFromJsonAsync<PassengerReadDto>();
        Assert.Equal("UpdatedName", patched?.FirstName);

        // --- 5. DELETE (soft) ---
        var deleteResponse = await _client.DeleteAsync($"/api/passengers/{passengerId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var verifyDelete = await _client.GetAsync($"/api/passengers/{passengerId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDelete.StatusCode);

        // --- 6. RESTORE ---
        var restoreResponse = await _client.PutAsync($"/api/passengers/{passengerId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        var finalGet = await _client.GetAsync($"/api/passengers/{passengerId}");
        Assert.Equal(HttpStatusCode.OK, finalGet.StatusCode);
    }
}
