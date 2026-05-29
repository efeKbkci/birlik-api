using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class StopMappingProfile : Profile
{
    public StopMappingProfile()
    {
        CreateMap<Stop, DetailedStopReadDto>();
        CreateMap<Stop, BasicStopReadDto>();
        CreateMap<Stop, StopDeleteIncludedDto>();

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
