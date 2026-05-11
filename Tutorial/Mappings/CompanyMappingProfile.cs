using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

// Profile sınıfından miras alması şarttır!
public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        // --- OKUMA (Entity'den DTO'ya) ---
        CreateMap<Company, CompanyReadDto>();
        CreateMap<Company, CompanyDeleteIncludedDto>();

        // --- YAZMA (DTO'dan Entity'ye) ---
        CreateMap<CompanyCreateDto, Company>();
        CreateMap<CompanyPatchDto,  Company>();

        // Not: İsimleri tam eşleşmeyen kolonlar varsa, onları da özel olarak buraya yazarak eşleştirebiliyoruz ama şu an isimlerin kusursuz.
    }
}