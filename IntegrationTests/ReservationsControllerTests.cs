using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Birlik.Shared.DTOs;
using Birlik.Shared.Enums;
using Xunit;

namespace IntegrationTests;

public class ReservationsControllerTests(InMemoryWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<(int companyId, int routeId, int vehicleId, int driverId, int stopId, int passengerId)> SetupReservationDependencies()
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
        var routeId = (await rRes.Content.ReadFromJsonAsync<RouteReadDto>(_jsonOptions))!.Id;

        var stopRes = await _client.PostAsJsonAsync("/api/stops", TestDataFactory.CreateNewStopObject(companyId, routeId));
        var stopId = (await stopRes.Content.ReadFromJsonAsync<DetailedStopReadDto>())!.Id;

        var pRes = await _client.PostAsJsonAsync("/api/passengers", TestDataFactory.CreateNewPassengerObject());
        var passengerId = (await pRes.Content.ReadFromJsonAsync<PassengerReadDto>())!.Id;

        return (companyId, routeId, vehicleId, driverId, stopId, passengerId);
    }

    [Fact]
    public async Task ReservationLifecycle_ShouldWorkCorrectly()
    {
        // ARRANGE - tüm bağımlılıkları hazırla
        var (companyId, routeId, vehicleId, driverId, stopId, passengerId) = await SetupReservationDependencies();

        var now = DateTime.UtcNow;
        var tripCreate = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddDays(1), (TripStatus)2);
        var tripRes = await _client.PostAsJsonAsync("/api/trips", tripCreate, _jsonOptions);
        var tripId = (await tripRes.Content.ReadFromJsonAsync<DetailedTripReadDashboardDto>(_jsonOptions))!.Id;

        // CREATE
        var createDto = TestDataFactory.CreateNewReservationObject(tripId, passengerId, stopId);
        var postResponse = await _client.PostAsJsonAsync("/api/reservations", createDto, _jsonOptions);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<Tutorial.Entities.Reservation>(_jsonOptions);
        Assert.NotNull(created);
        var reservationId = created!.Id;

        // READ by id
        var getResponse = await _client.GetAsync($"/api/reservations/{reservationId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var detailed = await getResponse.Content.ReadFromJsonAsync<DetailedReservationReadDto>(_jsonOptions);
        Assert.NotNull(detailed);

        // PATCH (update status)
        var patchDto = new ReservationPatchDto { ReservationStatus = ReservationStatus.Confirmed, PassengerStatus = PassengerStatus.Boarded };
        var patchResponse = await _client.PatchAsJsonAsync($"/api/reservations/{reservationId}", patchDto, _jsonOptions);
        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

        var verify = await _client.GetAsync($"/api/reservations/{reservationId}");
        var updated = await verify.Content.ReadFromJsonAsync<DetailedReservationReadDto>(_jsonOptions);
        Assert.Equal(ReservationStatus.Confirmed, updated?.ReservationStatus);
    }

    [Fact]
    public async Task GetReservations_ByTrip_ShouldReturnAll()
    {
        var (companyId, routeId, vehicleId, driverId, stopId, passengerId) = await SetupReservationDependencies();

        var now = DateTime.UtcNow;
        var tripCreate = TestDataFactory.CreateNewTripObject(companyId, routeId, vehicleId, driverId, now.AddDays(1), (TripStatus)2);
        var tripRes = await _client.PostAsJsonAsync("/api/trips", tripCreate, _jsonOptions);
        var tripId = (await tripRes.Content.ReadFromJsonAsync<DetailedTripReadDashboardDto>(_jsonOptions))!.Id;

        // Create two different passengers + reservations
        var pRes2 = await _client.PostAsJsonAsync("/api/passengers", TestDataFactory.CreateNewPassengerObject());
        var passengerId2 = (await pRes2.Content.ReadFromJsonAsync<PassengerReadDto>(_jsonOptions))!.Id;

        var res1 = TestDataFactory.CreateNewReservationObject(tripId, passengerId, stopId);
        var res2 = TestDataFactory.CreateNewReservationObject(tripId, passengerId2, stopId);

        await _client.PostAsJsonAsync("/api/reservations", res1, _jsonOptions);
        await _client.PostAsJsonAsync("/api/reservations", res2, _jsonOptions);

        var getByTrip = await _client.GetAsync($"/api/reservations/trip/{tripId}");
        Assert.Equal(HttpStatusCode.OK, getByTrip.StatusCode);

        var list = await getByTrip.Content.ReadFromJsonAsync<List<BasicReservationReadDto>>(_jsonOptions);
        Assert.NotNull(list);
        Assert.Equal(2, list!.Count);
    }
}
