using AutoMapper;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class TripMappingProfile : Profile
{
    public TripMappingProfile()
    {
        CreateMap<Trip, DetailedTripReadDashboardDto>();
        CreateMap<Trip, BasicTripReadDashboardDto>();
        CreateMap<Trip, TripReadPassengerDto>();
        CreateMap<Trip, TripDeleteIncludedDto>();
        CreateMap<Trip, TripListDto>()
            .ForMember(dest => dest.RouteName, opt => opt.MapFrom(
                src => src.Route.DepartureCity.Name + " - " + src.Route.ArrivalCity.Name
            ))
            .ForMember(dest => dest.VehiclePlate, opt => opt.MapFrom(src => src.Vehicle.PlateNumber))
            .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver.FirstName + " " + src.Driver.LastName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.TripStatus));

        CreateMap<TripCreateDto, Trip>();
        CreateMap<TripPatchDto, Trip>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom((src, dest) => src.CompanyId ?? dest.CompanyId))
            .ForMember(dest => dest.RouteId, opt => opt.MapFrom((src, dest) => src.RouteId ?? dest.RouteId))
            .ForMember(dest => dest.VehicleId, opt => opt.MapFrom((src, dest) => src.VehicleId ?? dest.VehicleId))
            .ForMember(dest => dest.DriverId, opt => opt.MapFrom((src, dest) => src.DriverId ?? dest.DriverId))
            .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom((src, dest) => src.DepartureTime ?? dest.DepartureTime))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom((src, dest) => src.AvailableCapacity ?? dest.Capacity))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom((src, dest) => src.BasePrice ?? dest.BasePrice))
            .ForMember(dest => dest.TripStatus, opt => opt.MapFrom((src, dest) => src.TripStatus ?? dest.TripStatus))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
