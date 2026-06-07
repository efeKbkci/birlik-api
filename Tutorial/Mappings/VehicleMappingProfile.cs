using AutoMapper;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class VehicleMappingProfile : Profile
{
    public VehicleMappingProfile() 
    {
        CreateMap<Vehicle, DetailedVehicleReadDto>();
        CreateMap<Vehicle, BasicVehicleReadDto>();
        CreateMap<Vehicle, VehicleDeleteIncludedDto>();
        CreateMap<Vehicle, VehicleListDto>();
        CreateMap<VehicleCreateDto, Vehicle>();
        CreateMap<VehiclePatchDto, Vehicle>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom((src, dest) => src.CompanyId ?? dest.CompanyId))
            .ForMember(dest => dest.DefaultDriverId, opt => opt.MapFrom((src, dest) => src.DefaultDriverId ?? dest.DefaultDriverId))
            .ForMember(dest => dest.Capacity, opt => opt.MapFrom((src, dest) => src.Capacity ?? dest.Capacity))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom((src, dest) => src.IsActive ?? dest.IsActive))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
