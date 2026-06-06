using System.Net;
using System.Net.Http.Json;
using Bogus;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Birlik.Shared.DTOs;
using Xunit;

namespace IntegrationTests;

public class AdminsControllerTests(InMemoryWebApplicationFactory factory) : IClassFixture<InMemoryWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task AdminFullLifecycle_ShouldWorkCorrectly()
    {
        // --- 0. BAĞIMLILIK: Şirket oluştur (Admin CompanyId gerektirir) ---
        var companyDto = TestDataFactory.CreateNewCompanyObject();
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        Assert.Equal(HttpStatusCode.Created, companyResponse.StatusCode);

        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<DetailedCompanyReadDto>();
        var companyId = createdCompany!.Id;

        // --- 1. ARRANGE: Admin oluşturma verisi ---
        var faker = new Faker("tr");
        var adminCreate = new AdminCreateDto
        {
            CompanyId = companyId,
            PhoneNumber = faker.Phone.PhoneNumber("05#########"),
            Email = faker.Internet.Email(),
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            Password = faker.Internet.Password()
        };

        // --- 2. CREATE (POST) ---
        var postResponse = await _client.PostAsJsonAsync("/api/admins", adminCreate);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdAdmin = await postResponse.Content.ReadFromJsonAsync<AdminReadDto>();
        Assert.NotNull(createdAdmin);
        var adminId = createdAdmin!.Id;

        // --- 3. READ (GET) ---
        var getResponse = await _client.GetAsync($"/api/admins/{adminId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetchedAdmin = await getResponse.Content.ReadFromJsonAsync<AdminReadDto>();
        Assert.Equal(adminCreate.FirstName, fetchedAdmin?.FirstName);

        // --- 4. LOGIN PREVIEW ---
        var loginDto = new AdminLoginDto { EmailOrPhone = adminCreate.Email, Password = adminCreate.Password };
        var loginResponse = await _client.PostAsJsonAsync("/api/admins/login-preview", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // --- 5. DELETE (Soft) ---
        var deleteResponse = await _client.DeleteAsync($"/api/admins/{adminId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // GET should now return 404
        var verifyDeleteResponse = await _client.GetAsync($"/api/admins/{adminId}");
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);

        // LOGIN should now be unauthorized (because soft-deleted admin filtered out)
        var loginAfterDeleteResponse = await _client.PostAsJsonAsync("/api/admins/login-preview", loginDto);
        Assert.Equal(HttpStatusCode.Unauthorized, loginAfterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAdmin_WithDuplicateEmailOrPhone_ShouldReturnConflict()
    {
        // Arrange: create company
        var companyDto = TestDataFactory.CreateNewCompanyObject();
        var companyResponse = await _client.PostAsJsonAsync("/api/companies", companyDto);
        Assert.Equal(HttpStatusCode.Created, companyResponse.StatusCode);

        var createdCompany = await companyResponse.Content.ReadFromJsonAsync<DetailedCompanyReadDto>();
        var companyId = createdCompany!.Id;

        var faker = new Faker("tr");
        var admin1 = new AdminCreateDto
        {
            CompanyId = companyId,
            PhoneNumber = faker.Phone.PhoneNumber("05#########"),
            Email = faker.Internet.Email(),
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            Password = faker.Internet.Password()
        };

        var admin2 = new AdminCreateDto
        {
            CompanyId = companyId,
            PhoneNumber = faker.Phone.PhoneNumber("05#########"),
            Email = faker.Internet.Email(),
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            Password = faker.Internet.Password()
        };

        // Act: create first admin
        var postResponse1 = await _client.PostAsJsonAsync("/api/admins", admin1);
        Assert.Equal(HttpStatusCode.Created, postResponse1.StatusCode);

        // Case A: duplicate email
        admin2.Email = admin1.Email;
        var postResponseDupEmail = await _client.PostAsJsonAsync("/api/admins", admin2);
        Assert.Equal(HttpStatusCode.Conflict, postResponseDupEmail.StatusCode);

        // Case B: duplicate phone
        admin2.Email = faker.Internet.Email();
        admin2.PhoneNumber = admin1.PhoneNumber;
        var postResponseDupPhone = await _client.PostAsJsonAsync("/api/admins", admin2);
        Assert.Equal(HttpStatusCode.Conflict, postResponseDupPhone.StatusCode);
    }
}
