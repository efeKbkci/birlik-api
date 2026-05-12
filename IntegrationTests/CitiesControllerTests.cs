using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tutorial.DTOs;
using Xunit;

namespace IntegrationTests;

// IClassFixture, API'ımızı sanal olarak ayağa kaldıran altyapıyı sağlar
public class CitiesControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact] // XUnit'e bunun çalıştırılması gereken bir test olduğunu söyler
    public async Task CityLifecycle_Create_Read_Delete_ShouldExecuteSuccessfully()
    {
        // --- 1. ARRANGE (Hazırlık) ---
        var randomCityName = "Test_City_" + Guid.NewGuid().ToString()[..5];
        var createDto = new CityCreateDto { Name = randomCityName };

        // --- 2. ACT & ASSERT (Oluşturma İşlemi) ---
        var postResponse = await _client.PostAsJsonAsync("/api/cities", createDto);

        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode); // 201 döndü mü?

        var createdCity = await postResponse.Content.ReadFromJsonAsync<CityReadDto>();
        Assert.NotNull(createdCity);

        var cityId = createdCity.Id; // Zincirleme test için ID'yi yakaladık

        // --- 3. ACT & ASSERT (Okuma İşlemi) ---
        var getResponse = await _client.GetAsync($"/api/cities/{cityId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode); // 200 döndü mü?

        var fetchedCity = await getResponse.Content.ReadFromJsonAsync<CityReadDto>();
        Assert.NotNull(fetchedCity);
        Assert.Equal(randomCityName, fetchedCity.Name); // İsimler eşleşiyor mu?

        // --- 4. ACT & ASSERT (Silme İşlemi) ---
        var deleteResponse = await _client.DeleteAsync($"/api/cities/{cityId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode); // 204 döndü mü?

        // --- 5. ACT & ASSERT (Silindiğini Doğrulama) ---
        var verifyDeleteResponse = await _client.GetAsync($"/api/cities/{cityId}");

        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode); // 404 döndü mü?
    }
}