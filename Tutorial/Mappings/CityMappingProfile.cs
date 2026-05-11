using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class CityMappingProfile : Profile
{
    public CityMappingProfile() 
    {
        // --- OKUMA (Entity'den DTO'ya) ---
        CreateMap<City, CityReadDto>();
        CreateMap<City, CityDeleteIncludedDto>();

        // --- YAZMA (DTO'dan Entity'ye) ---
        CreateMap<CityCreateDto, City>();
        CreateMap<CityPatchDto, City>();
    }
}
