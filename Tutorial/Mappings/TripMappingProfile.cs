using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class TripMappingProfile : Profile
{
    public TripMappingProfile()
    {
        CreateMap<Trip, TripReadDashboardDto>();
        CreateMap<Trip, TripReadPassengerDto>();
        CreateMap<Trip, TripDeleteIncludedDto>();

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
