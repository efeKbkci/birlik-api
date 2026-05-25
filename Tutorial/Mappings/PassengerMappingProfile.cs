using AutoMapper;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class PassengerMappingProfile : Profile
{
    public PassengerMappingProfile()
    {
        CreateMap<Passenger, PassengerReadDto>();
        CreateMap<Passenger, PassengerDeleteIncludedDto>();

        CreateMap<PassengerCreateDto, Passenger>();

        CreateMap<PassengerPatchDto, Passenger>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
