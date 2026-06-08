using AutoMapper;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Birlik.Shared.Enums;
using Tutorial.Entities;

namespace Tutorial.Mappings
{
    public class DriversMappingProfile : Profile
    {
        public DriversMappingProfile() 
        {
            // --- OKUMA (Entity'den DTO'ya) ---
            CreateMap<Driver, DetailedDriverReadDto>();
            CreateMap<Driver, DriverListDto>();
            CreateMap<Driver, BasicDriverReadDto>();
            CreateMap<Driver, DriverDeleteIncludedDto>();

            // --- YAZMA (DTO'dan Entity'ye) ---
            CreateMap<DriverCreateDto, Driver>();
            CreateMap<DriverPatchDto,  Driver>()
                // integer, boolean gibi değer tipleri null olamaz.
                // int? CompanyId = null olarak kullanıcıdan geldiyse AutoMapper onu 0 değerine dönüştürür.
                // DriverStatus? Status = null olarak geldiyse ForAllMembers kuralı sayesinde dest değeri korunur.
                // Bu otomatik dönüştürme işlemini istemediğimiz için null olmayan değeri kullan diyoruz.
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom((src, dest) => src.CompanyId ?? dest.CompanyId))
                // Status mapping will be handled by AutoMapper conventions if the DTO exposes a matching property.
                // Tüm property'ler için şu kuralı uygula: Sadece kaynak değer (srcMember) null DEĞİLSE kopyala.
                // Aksi durumda entity değerini değiştirme.
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); 
        }
    }
}
