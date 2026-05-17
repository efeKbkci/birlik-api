using System.Net;
using System.Net.Http.Json;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

public class CompaniesControllerTests(InMemoryWebApplicationFactory factory) : IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CompanyFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 1. ARRANGE: Veri Hazırlığı ---
        var properRequest = TestDataFactory.CreateNewCompanyObject();
        var conflictedRequest = TestDataFactory.CreateNewCompanyObject();

        conflictedRequest.ContactPhone = properRequest.ContactPhone;

        // --- 2. CREATE: Şirket Oluşturma (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/companies", properRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var conflictedResponse = await _client.PostAsJsonAsync("/api/companies", conflictedRequest);
        Assert.Equal(HttpStatusCode.Conflict, conflictedResponse.StatusCode);

        var createdCompany = await postResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        Assert.NotNull(createdCompany);
        var companyId = createdCompany.Id;

        // --- 3. READ: Oluşturulan Veriyi Doğrulama (GET) ---
        var getResponse = await _client.GetAsync($"/api/companies/{companyId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedCompany = await getResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        Assert.Equal(properRequest.CompanyName, fetchedCompany?.CompanyName);

        // --- 4. UPDATE: Bilgileri Güncelleme (PATCH) ---
        var newLocation = "Ankara";
        var patchDto = new CompanyPatchDto { Location = newLocation };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/companies/{companyId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        // Güncelleme kontrolü
        var getUpdatedResponse = await _client.GetAsync($"/api/companies/{companyId}");
        var updatedCompany = await getUpdatedResponse.Content.ReadFromJsonAsync<CompanyReadDto>();
        Assert.Equal(newLocation, updatedCompany?.Location);

        // --- 5. DELETE: Yumuşak Silme (DELETE) ---
        var deleteResponse = await _client.DeleteAsync($"/api/companies/{companyId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Silindiği için standart GET isteği 404 dönmeli
        var verifyDeleteResponse = await _client.GetAsync($"/api/companies/{companyId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);

        // --- 6. RESTORE: Veriyi Kurtarma (PUT) ---
        var restoreResponse = await _client.PutAsync($"/api/companies/{companyId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restoreResponse.StatusCode);

        // Kurtarıldıktan sonra tekrar 200 dönmeli
        var finalVerifyResponse = await _client.GetAsync($"/api/companies/{companyId}");
        Assert.Equal(HttpStatusCode.OK, finalVerifyResponse.StatusCode);
    }
}