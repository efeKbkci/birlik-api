using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Birlik.Shared.Enums;
using Bogus;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.Controllers;
using Tutorial.Entities;
using Xunit;

namespace IntegrationTests;


public class TripsControllerTests(InMemoryWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(int companyId, int routeId, int vehicleId, int driverId)> SetupTripDependencies()
    {
        var cRes = await _client.PostAsJsonAsync("/api/companies", TestDataFactory.CreateNewCompanyObject());
        var companyId = (await cRes.Content.ReadFromJsonAsync<DetailedCompanyReadDto>())!.Id;

        var dRes = await _client.PostAsJsonAsync("/api/drivers", TestDataFactory.CreateNewDriverObject(companyId));
        var driverId = (await dRes.Content.ReadFromJsonAsync<DetailedDriverReadDto>())!.Id;

        var vRes = await _client.PostAsJsonAsync("/api/vehicles", TestDataFactory.CreateNewVehicleObject(companyId, driverId));
        var vehicleId = (await vRes.Content.ReadFromJsonAsync<DetailedVehicleReadDto>())!.Id;

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

        var createdTrip = await postResponse.Content.ReadFromJsonAsync<DetailedTripReadDashboardDto>(_jsonOptions); // Dönüş tipini kontrol et
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
        var updatedTrip = await verifyPatchRes.Content.ReadFromJsonAsync<DetailedTripReadDashboardDto>(_jsonOptions);
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
        var filteredTrips = await response.Content.ReadFromJsonAsync<List<BasicTripReadDashboardDto>>(_jsonOptions);

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
        foreach (var trip in (List<TripCreateDto>)[trip1, trip2, trip3, trip4, trip5])
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

    /// <summary>
    /// Create a fresh in-memory AppDbContext and seed it with deterministic test data for a single company.
    /// The seeded data includes trips (some today, one cancelled, one other-date), vehicles and drivers with varying IsActive/IsDeleted flags.
    /// </summary>
    private static AppDbContext CreateAndSeedContext(int seededCompanyId, out int expectedTodayTrips, out int expectedActiveVehicles, out int expectedActiveDrivers, out List<TripListDto> expectedOrderedTrips)
    {
        var dbName = $"TripsDb_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var ctx = new AppDbContext(options);

        // Clear (ensures clean state) - not strictly necessary for unique DB but safe
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();

        // Seed helper faker
        var faker = new Faker("en");

        // Cities
        var c1 = new City { Id = 1, Name = "CityA" };
        var c2 = new City { Id = 2, Name = "CityB" };
        ctx.Cities.AddRange(c1, c2);

        // Route
        var route = new Route
        {
            Id = 1,
            DepartureCity = c1,
            ArrivalCity = c2,
            DepartureCityId = c1.Id,
            ArrivalCityId = c2.Id,
            EstimatedDuration = 120
        };
        ctx.Routes.Add(route);

        // Drivers
        var activeDriver = new Driver
        {
            Id = 1,
            CompanyId = seededCompanyId,
            FirstName = "Active",
            LastName = "Driver",
            IsActive = true,
            IsDeleted = false
        };
        var inactiveDriver = new Driver
        {
            Id = 2,
            CompanyId = seededCompanyId,
            FirstName = "Inactive",
            LastName = "Driver",
            IsActive = false,
            IsDeleted = false
        };
        ctx.Drivers.AddRange(activeDriver, inactiveDriver);

        // Vehicles
        var vehicleActive = new Vehicle
        {
            Id = 1,
            CompanyId = seededCompanyId,
            PlateNumber = faker.Vehicle.Vin().Substring(0, 8),
            IsActive = true,
            IsDeleted = false
        };
        var vehicleDeleted = new Vehicle
        {
            Id = 2,
            CompanyId = seededCompanyId,
            PlateNumber = faker.Vehicle.Vin().Substring(0, 8),
            IsActive = true,
            IsDeleted = true
        };
        ctx.Vehicles.AddRange(vehicleActive, vehicleDeleted);

        // Today and other dates
        var today = DateTime.UtcNow.Date;
        var trip1 = new Trip
        {
            Id = 1,
            CompanyId = seededCompanyId,
            Route = route,
            RouteId = route.Id,
            Vehicle = vehicleActive,
            VehicleId = vehicleActive.Id,
            Driver = activeDriver,
            DriverId = activeDriver.Id,
            DepartureTime = today.AddHours(10),
            TripStatus = TripStatus.OnSale
        };
        var trip2 = new Trip
        {
            Id = 2,
            CompanyId = seededCompanyId,
            Route = route,
            RouteId = route.Id,
            Vehicle = vehicleActive,
            VehicleId = vehicleActive.Id,
            Driver = activeDriver,
            DriverId = activeDriver.Id,
            DepartureTime = today.AddHours(15),
            TripStatus = TripStatus.OnSale
        };
        // Trip on different date but same company and not canceled
        var trip3 = new Trip
        {
            Id = 3,
            CompanyId = seededCompanyId,
            Route = route,
            RouteId = route.Id,
            Vehicle = vehicleActive,
            VehicleId = vehicleActive.Id,
            Driver = activeDriver,
            DriverId = activeDriver.Id,
            DepartureTime = today.AddDays(-1).AddHours(8),
            TripStatus = TripStatus.Scheduled
        };
        // Cancelled trip (should be excluded everywhere)
        var cancelledTrip = new Trip
        {
            Id = 4,
            CompanyId = seededCompanyId,
            Route = route,
            RouteId = route.Id,
            Vehicle = vehicleActive,
            VehicleId = vehicleActive.Id,
            Driver = activeDriver,
            DriverId = activeDriver.Id,
            DepartureTime = today.AddHours(9),
            TripStatus = TripStatus.Canceled
        };
        // Trip for another company (should not be counted)
        var otherCompanyTrip = new Trip
        {
            Id = 5,
            CompanyId = seededCompanyId + 1,
            Route = route,
            RouteId = route.Id,
            Vehicle = vehicleActive,
            VehicleId = vehicleActive.Id,
            Driver = activeDriver,
            DriverId = activeDriver.Id,
            DepartureTime = today.AddHours(11),
            TripStatus = TripStatus.OnSale
        };

        ctx.Trips.AddRange(trip1, trip2, trip3, cancelledTrip, otherCompanyTrip);

        ctx.SaveChanges();

        // Expected aggregates
        expectedTodayTrips = 2; // trip1 & trip2 (cancelledTrip excluded)
        expectedActiveVehicles = 1; // vehicleActive (vehicleDeleted is IsDeleted=true)
        expectedActiveDrivers = 1; // activeDriver only

        // Expected ordered trips (non-canceled for company, ordered desc by DepartureTime)
        var expectedTrips = new List<TripListDto>
            {
                new TripListDto
                {
                    Id = trip2.Id,
                    DepartureTime = trip2.DepartureTime,
                    RouteName = $"{trip2.Route.DepartureCity.Name}-{trip2.Route.ArrivalCity.Name}",
                    VehiclePlate = trip2.Vehicle.PlateNumber,
                    DriverName = $"{trip2.Driver.FirstName} {trip2.Driver.LastName}",
                    Status = trip2.TripStatus
                },
                new TripListDto
                {
                    Id = trip1.Id,
                    DepartureTime = trip1.DepartureTime,
                    RouteName = $"{trip1.Route.DepartureCity.Name}-{trip1.Route.ArrivalCity.Name}",
                    VehiclePlate = trip1.Vehicle.PlateNumber,
                    DriverName = $"{trip1.Driver.FirstName} {trip1.Driver.LastName}",
                    Status = trip1.TripStatus
                },
                new TripListDto
                {
                    Id = trip3.Id,
                    DepartureTime = trip3.DepartureTime,
                    RouteName = $"{trip3.Route.DepartureCity.Name}-{trip3.Route.ArrivalCity.Name}",
                    VehiclePlate = trip3.Vehicle.PlateNumber,
                    DriverName = $"{trip3.Driver.FirstName} {trip3.Driver.LastName}",
                    Status = trip3.TripStatus
                }
            };

        expectedOrderedTrips = expectedTrips;

        return ctx;
    }

}