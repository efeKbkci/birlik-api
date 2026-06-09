using AutoMapper;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class StopMappingProfile : Profile
{
    public StopMappingProfile()
    {
        CreateMap<Stop, DetailedStopReadDto>();
        CreateMap<Stop, BasicStopReadDto>();
        CreateMap<Stop, StopDeleteIncludedDto>();
        // List view mapping for management page
        CreateMap<Stop, StopListDto>()
            .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route.DepartureCity.Name + " - " + src.Route.ArrivalCity.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.StopName))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive && !src.IsDeleted ? "Active" : "Inactive"));

        CreateMap<StopCreateDto, Stop>()
             .ForMember(dest => dest.TimeOffsetMins, opt => opt.MapFrom((src, dest) => src.TimeOffsetMins ?? -1));

        CreateMap<StopPatchDto, Stop>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom((src, dest) => src.CompanyId ?? dest.CompanyId))
            .ForMember(dest => dest.RouteId, opt => opt.MapFrom((src, dest) => src.RouteId ?? dest.RouteId))
            .ForMember(dest => dest.StopOrder, opt => opt.MapFrom((src, dest) => src.StopOrder ?? dest.StopOrder))
            .ForMember(dest => dest.TimeOffsetMins, opt => opt.MapFrom((src, dest) => src.TimeOffsetMins ?? dest.TimeOffsetMins))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom((src, dest) => src.IsActive ?? dest.IsActive))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
