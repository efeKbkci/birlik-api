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
            CreateMap<DriverPatchDto,  Driver>()
                // integer, boolean gibi değer tipleri null olamaz.
                // int? CompanyId = null olarak kullanıcıdan geldiyse AutoMapper onu 0 değerine dönüştürür. 
                // bool? IsActive = null olarak geldiyse AutoMapper onu false değerine dönüştürür. 
                // Bu otomatik dönüştürme işlemini istemediğimiz için null olmayan değeri kullan diyoruz. 
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom((src, dest) => src.CompanyId ?? dest.CompanyId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom((src, dest) => src.IsActive ?? dest.IsActive))
                // Tüm property'ler için şu kuralı uygula: Sadece kaynak değer (srcMember) null DEĞİLSE kopyala.
                // Aksi durumda entity değerini değiştirme.
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 
        }
    }
}
