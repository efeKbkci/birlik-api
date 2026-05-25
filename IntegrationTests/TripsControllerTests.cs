using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Tutorial.DTOs;
using Tutorial.Enums;
using Xunit;

namespace IntegrationTests;

public class TripsControllerTests(InMemoryWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(int companyId, int routeId, int vehicleId, int driverId)> SetupTripDependencies()
    {
        var cRes = await _client.PostAsJsonAsync("/api/companies", TestDataFactory.CreateNewCompanyObject());
        var companyId = (await cRes.Content.ReadFromJsonAsync<CompanyReadDto>())!.Id;

        var dRes = await _client.PostAsJsonAsync("/api/drivers", TestDataFactory.CreateNewDriverObject(companyId));
        var driverId = (await dRes.Content.ReadFromJsonAsync<DriverReadDto>())!.Id;

        var vRes = await _client.PostAsJsonAsync("/api/vehicles", TestDataFactory.CreateNewVehicleObject(companyId, driverId));
        var vehicleId = (await vRes.Content.ReadFromJsonAsync<VehicleReadDto>())!.Id;

        var city1Res = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city1Id = (await city1Res.Content.ReadFromJsonAsync<CityReadDto>())!.Id;

        var city2Res = await _client.PostAsJsonAsync("/api/cities", TestDataFactory.CreateNewCityObject());
        var city2Id = (await city2Res.Content.ReadFromJsonAsync<CityReadDto>())!.Id;

        var rRes = await _client.PostAsJsonAsync("/api/routes", TestDataFactory.CreateNewRouteObject(city1Id, city2Id));
        var routeId = (await rRes.Content.ReadFromJsonAsync<RouteReadDto>())!.Id;

        return (companyId, routeId, vehicleId, driverId);
    }

    [Fact]
    public async Task TripLifecycle_ShouldWorkCorrectly()
    {
        // 1. ARRANGE
        var (companyId, routeId, vehicleId, driverId) = await SetupTripDependencies();
        var now = DateTime.UtcNow;
        var createDto = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddDays(2), (TripStatus)2); // 2 = OnSale varsayıyoruz

        // 2. CREATE (POST)
        var postResponse = await _client.PostAsJsonAsync("/api/trips", createDto, _jsonOptions);
        // IntegrationTestBase içerisindeki JSON - Enum class ayarı sayesinde createDto içerisindeki enum veri yapısı bir string olarak JSON içerisinde serileştirilecek.
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdTrip = await postResponse.Content.ReadFromJsonAsync<TripReadDashboardDto>(_jsonOptions); // Dönüş tipini kontrol et
        Assert.NotNull(createdTrip);
        var tripId = createdTrip.Id;

        // 3. READ (GET by ID)
        var getResponse = await _client.GetAsync($"/api/trips/{tripId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        // 4. UPDATE (PATCH)
        var patchDto = new TripPatchDto { TripStatus = (TripStatus)5 }; // 5 = Cancelled varsayıyoruz
        var patchResponse = await _client.PatchAsJsonAsync($"/api/trips/{tripId}", patchDto);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        // Doğrulama
        var verifyPatchRes = await _client.GetAsync($"/api/trips/{tripId}");
        var updatedTrip = await verifyPatchRes.Content.ReadFromJsonAsync<TripReadDashboardDto>(_jsonOptions);
        Assert.Equal((TripStatus)5, updatedTrip?.TripStatus);
    }

    [Fact]
    public async Task GetTrips_DashboardView_ShouldFilterCorrectly()
    {
        // 1. ARRANGE
        var (companyId, routeId, vehicleId, driverId) = await SetupTripDependencies();
        var now = DateTime.UtcNow;

        // İki farklı sefer ekliyoruz
        await _client.PostAsJsonAsync("/api/trips", TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddDays(1), (TripStatus)1), _jsonOptions); // Aktif
        await _client.PostAsJsonAsync("/api/trips", TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddDays(2), (TripStatus)5), _jsonOptions); // İptal

        // 2. ACT - Sadece aktif olanları filtrele (Status = 1)
        var response = await _client.GetAsync($"/api/trips/dashboardView?companyId={companyId}&status=1");

        // 3. ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var filteredTrips = await response.Content.ReadFromJsonAsync<List<TripReadDashboardDto>>(_jsonOptions);

        Assert.NotNull(filteredTrips);
        Assert.Single(filteredTrips); // 2 sefer ekledik ama 1 tanesi gelmeli (Çünkü diğeri iptal)
        Assert.Equal((TripStatus)1, filteredTrips.First().TripStatus);
    }

    [Fact]
    public async Task GetTrips_PassengerView_ShouldApplyBusinessRules()
    {
        // 1. ARRANGE
        var (companyId, routeId, vehicleId, driverId) = await SetupTripDependencies();
        var now = DateTime.UtcNow;

        // Sefer 1: BUGÜN, Satışta, Kapasite var (GÖSTERİLMELİ)
        var trip1 = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddHours(2), (TripStatus)2); // OnSale

        // Sefer 2: BUGÜN, Satışta ama KAPASİTE DOLU (GİZLENMELİ - Edge Case!)
        var trip2 = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddHours(3), (TripStatus)2);
        trip2.Capacity = 40;
        trip2.PassengerNumbers = 40;

        // Sefer 3: YARIN, Satışta (BUGÜN filtresinde GİZLENMELİ, YARIN filtresinde GÖSTERİLMELİ)
        var trip3 = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.Date.AddDays(1).AddHours(5), (TripStatus)2);

        // Sefer 4: GELECEK, Satışta (TARİH filtresinden GİZLENMELİ, SATIŞTA filtresinden GÖSTERİLMELİ) 
        var trip4 = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.Date.AddDays(3), (TripStatus)2);

        // Sefer 5: BUGÜN, Satışta Değil, Aktif (SATIŞTA filtresinden GİZLENMELİ, TARİH filtresinden GÖSTERİLMELİ) 
        var trip5 = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddHours(1), (TripStatus)1);

        // Seferleri Veritabanına Yaz
        foreach (var trip in (List<TripCreateDto>) [trip1, trip2, trip3, trip4, trip5])
        {
            await _client.PostAsJsonAsync("/api/trips", trip, _jsonOptions);
        }

        // 2. ACT - Yolcu "Bugün" (TripDaySelection.Today = 1) için arama yapıyor
        var todayResponse = await _client.GetAsync($"/api/trips/passengerView?CompanyId={companyId}&RouteId={routeId}&DaySelection=Today");

        // 3. ASSERT - Bugün
        Assert.Equal(HttpStatusCode.OK, todayResponse.StatusCode);
        var todayTrips = await todayResponse.Content.ReadFromJsonAsync<List<TripReadPassengerDto>>(_jsonOptions);

        Assert.NotNull(todayTrips);
        Assert.Contains(todayTrips, t => t.DepartureTime.Day == now.Day); // Bugünün seferi gelmeli
        Assert.DoesNotContain(todayTrips, t => t.DepartureTime.Day == now.AddDays(1).Day); // Yarının seferi KESİNLİKLE gelmemeli
        Assert.Single(todayTrips);

        // 4. ACT - Yolcu "Yarın" (TripDaySelection.Tomorrow = 2) için arama yapıyor
        var tomorrowResponse = await _client.GetAsync($"/api/trips/passengerView?CompanyId={companyId}&RouteId={routeId}&DaySelection=Tomorrow");

        // 5. ASSERT - Yarın
        Assert.Equal(HttpStatusCode.OK, tomorrowResponse.StatusCode);
        var tomorrowTrips = await tomorrowResponse.Content.ReadFromJsonAsync<List<TripReadPassengerDto>>(_jsonOptions);

        Assert.NotNull(tomorrowTrips);
        Assert.Contains(tomorrowTrips, t => t.DepartureTime.Day == now.AddDays(1).Day); // Yarının seferi gelmeli
        Assert.DoesNotContain(tomorrowTrips, t => t.DepartureTime.Day == now.Day); // Bugünün seferi KESİNLİKLE gelmemeli
        Assert.Single(tomorrowTrips);
    }
}