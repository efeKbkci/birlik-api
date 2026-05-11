using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings
{
    public class DriversMappingProfile : Profile
    {
        public DriversMappingProfile() 
        {
            // --- OKUMA (Entity'den DTO'ya) ---
            CreateMap<Driver, DriverReadDto>();
            CreateMap<Driver, DriverDeleteIncludedDto>();

            // --- YAZMA (DTO'dan Entity'ye) ---
            CreateMap<DriverCreateDto, Driver>();
            CreateMap<DriverPatchDto,  Driver>();
        }
    }
}
