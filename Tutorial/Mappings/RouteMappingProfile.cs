using AutoMapper;
using Tutorial.DTOs;
using Route = Tutorial.Entities.Route;

namespace Tutorial.Mappings
{
    public class RouteMappingProfile : Profile
    {
        public RouteMappingProfile() 
        {
            // --- OKUMA (Entity'den DTO'ya) ---
            CreateMap<Route, RouteReadDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.DepartureCity.Name} - {src.ArrivalCity.Name}"));
            
            CreateMap<Route, RouteDeleteIncludedDto>();

            // --- YAZMA (DTO'dan Entity'ye) ---
            CreateMap<RouteCreateDto, Route>();
            CreateMap<RoutePatchDto, Route>()
                .ForMember(dest => dest.ArrivalCityId, opt => opt.MapFrom((src, dest) => src.ArrivalCityId ?? dest.ArrivalCityId))
                .ForMember(dest => dest.DepartureCityId, opt => opt.MapFrom((src, dest) => src.DepartureCityId ?? dest.DepartureCityId))
                .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom((src, dest) => src.EstimatedDuration ?? dest.EstimatedDuration));
        }
    }
}
