using AutoMapper;
using Birlik.Shared.DTOs;
using Tutorial.Entities;

namespace Tutorial.Mappings;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<Reservation, DetailedReservationReadDto>()
            // 1. ADIM: Hangi hedef özelliği dolduracağımızı seçiyoruz
            .ForMember(dest => dest.RouteName, opt =>
                // 2. ADIM: Bu özelliğin kaynağını (Source) formüle ediyoruz
                opt.MapFrom(src =>
                    // 3. ADIM: 3 katman derine inip string birleştirme yapıyoruz
                    src.Trip.Route.DepartureCity.Name + " - " + src.Trip.Route.ArrivalCity.Name
                )
            )
            .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.Trip.Vehicle.PlateNumber))
            .ForMember(dest => dest.BasePrice, opt => opt.MapFrom(src => src.Trip.BasePrice))
            .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Trip.DepartureTime))
            .ForMember(dest => dest.PassengerName, opt => opt.MapFrom(src => src.Passenger.FirstName + " " + src.Passenger.LastName))
            .ForMember(dest => dest.PassengerNumber, opt => opt.MapFrom(src => src.Passenger.PhoneNumber))
            .ForMember(dest => dest.PickupStopName, opt => opt.MapFrom(src => src.PickupStop.StopName));

        CreateMap<Reservation, BasicReservationReadDto>()
            .ForMember(dest => dest.RouteName, opt =>
                opt.MapFrom(src =>
                    src.Trip.Route.DepartureCity.Name + " - " + src.Trip.Route.ArrivalCity.Name
                )
            )
            .ForMember(dest => dest.PassengerName, opt => opt.MapFrom(src => src.Passenger.FirstName + " " + src.Passenger.LastName))
            .ForMember(dest => dest.PassengerNumber, opt => opt.MapFrom(src => src.Passenger.PhoneNumber));

        CreateMap<ReservationCreateDto, Reservation>();

        CreateMap<ReservationPatchDto, Reservation>()
            .ForMember(dest => dest.TripId, opt => opt.MapFrom((src, dest) => src.TripId ?? dest.TripId))
            .ForMember(dest => dest.PickupStopId, opt => opt.MapFrom((src, dest) => src.PickupStopId ?? dest.PickupStopId))
            .ForMember(dest => dest.ReservationStatus, opt => opt.MapFrom((src, dest) => src.ReservationStatus ?? dest.ReservationStatus))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
