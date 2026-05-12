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
        CreateMap<CompanyPatchDto,  Company>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom((src, dest) => src.IsActive ?? dest.IsActive))
            // Tüm property'ler için şu kuralı uygula: Sadece kaynak değer (srcMember) null DEĞİLSE kopyala.
            // Aksi durumda entity değerini değiştirme.
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 

        // Not: İsimleri tam eşleşmeyen kolonlar varsa, onları da özel olarak buraya yazarak eşleştirebiliyoruz ama şu an isimlerin kusursuz.
    }
}