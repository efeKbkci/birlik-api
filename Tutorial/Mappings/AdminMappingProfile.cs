using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<Admin, AdminReadDto>();

        // Gelen düz şifreyi Entity'nin PasswordHash alanına bağlıyoruz
        CreateMap<AdminCreateDto, Admin>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password));
    }
}