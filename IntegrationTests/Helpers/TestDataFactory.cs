using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tutorial.DTOs;
using Tutorial.Entities;
using Tutorial.Enums;

namespace IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static CompanyCreateDto CreateNewCompanyObject()
    {
        var faker = new Bogus.Faker("tr");

        var randomName = faker.Company.CompanyName();
        var randomPhoneNumber = faker.Phone.PhoneNumber();
        var randomLocation = faker.Address.City();

        var createDto = new CompanyCreateDto
        {
            CompanyName = randomName,
            ContactPhone = randomPhoneNumber,
            Location = randomLocation,
            IsActive = true
        };

        return createDto;
    }

    public static DriverCreateDto CreateNewDriverObject(int companyId)
    {
        var faker = new Bogus.Faker("tr"); 

        var randomFirstName = faker.Name.FirstName();
        var randomLastName = faker.Name.LastName();
        var randomPhoneNumber = faker.Phone.PhoneNumber();
        var randomPassword = faker.Internet.Password(); 

        var createDto = new DriverCreateDto
        {
            CompanyId = companyId,
            FirstName = randomFirstName,
            LastName = randomLastName,
            PhoneNumber = randomPhoneNumber,
            PasswordHash = randomPassword,
            IsActive = true
        };

        return createDto;
    }

    public static CityCreateDto CreateNewCityObject()
    {
        var randomCityName = new Bogus.Faker("tr").Address.City();
        
        var createDto = new CityCreateDto { Name = randomCityName };

        return createDto;
    }

    public static RouteCreateDto CreateNewRouteObject(int departureId, int arrivalId)
    {
        var randomDuration = new Random().Next(60, 600);

        var createDto = new RouteCreateDto
        {
            DepartureCityId = departureId,
            ArrivalCityId = arrivalId,
            EstimatedDuration = randomDuration
        };

        return createDto;
    }

    public static VehicleCreateDto CreateNewVehicleObject(int companyId, int driverId)
    {
        var faker = new Bogus.Faker("tr");

        var randomCapacity = faker.Random.Number(15, 55);

        // 2. PLAKA ÜRETİMİ (Örn: 34 ABC 123)
        var cityCode = faker.Random.Number(1, 81).ToString("D2");
        var letterGroup = faker.Random.String2(faker.Random.Number(1, 3), "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        var numberGroup = faker.Random.Number(10, 9999);
        var randomPlate = $"{cityCode} {letterGroup} {numberGroup}";

        var createDto = new VehicleCreateDto
        {
            CompanyId = companyId,
            DefaultDriverId = driverId,
            PlateNumber = randomPlate,
            Capacity = randomCapacity,
            IsActive = true
        };

        return createDto;
    }

    public static StopCreateDto CreateNewStopObject(int companyId, int routeId)
    {
        var faker = new Bogus.Faker("tr");
        
        var createDto = new StopCreateDto
        {
            CompanyId = companyId,
            RouteId = routeId,
            StopName = faker.Address.StreetName(),
            StopOrder = faker.Random.Number(1, 10),
            TimeOffsetMins = faker.Random.Number(15, 60),
            IsActive = true
        };

        return createDto;
    }

    public static TripCreateDto CreateNewTripObject(int companyId, int routeId, int vehicleId, int driverId, DateTime departureTime, TripStatus tripStatus)
    {
        var faker = new Bogus.Faker("tr");
        var randomCapacity = faker.Random.Number(15, 55);
        var randomBasePrice = faker.Random.Number(50, 500);

        var createDto = new TripCreateDto
        {
            CompanyId = companyId,
            RouteId = routeId,
            VehicleId = vehicleId,
            DriverId = driverId,
            DepartureTime = departureTime,
            Capacity = randomCapacity,
            BasePrice = randomBasePrice,
            TripStatus = tripStatus
        };

        return createDto;
    } 
}
