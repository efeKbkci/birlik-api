using System.ComponentModel.DataAnnotations;
using Tutorial.Enums;

namespace Tutorial.DTOs;

public class DetailedTripReadDashboardDto // Yazýhanenin göreceði bilgiler
{
    // CompanyName niteliði kaldýrýldý. Firma zaten sadece kendi seferlerini görebilir. Yolcu da sadece konuþtuðu firmanýn seferlerini görebilir.
    public int Id { get; set; }
    public string RouteName { get; set; }
    public string VehiclePlate { get; set; }
    public string DriverName { get; set; }
    public DateTime DepartureTime { get; set; }
    public int Capacity { get; set; }
    public int PassengerNumbers { get; set; }
    public int BasePrice { get; set; }
    public TripStatus TripStatus { get; set; }
}

public class BasicTripReadDashboardDto // Yazýhanenin liste görüntüsünde daha az alan göster
{
    public int Id { get; set; }
    public string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public TripStatus TripStatus { get; set; }
}

public class TripReadPassengerDto // Yolcunun seferleri sorgularken göreceði bilgiler
{
    public int Id { get; set; }
    public string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public int BasePrice { get; set; }
}

public class TripDeleteIncludedDto : BasicTripReadDashboardDto
{
    public bool IsDeleted { get; set; }
}

public class TripCreateDto
{
    public int CompanyId { get; set; }
    public int RouteId { get; set; }
    public int VehicleId { get; set; }
    public int DriverId { get; set; }
    public DateTime DepartureTime { get; set; }
    public int Capacity { get; set; }
    public int PassengerNumbers { get; set; } = 0;
    public int BasePrice { get; set; }
    public TripStatus TripStatus { get; set; }
}

public class TripPatchDto
{
    public int? CompanyId { get; set; }
    public int? RouteId { get; set; }
    public int? VehicleId { get; set; }
    public int? DriverId { get; set; }
    public DateTime? DepartureTime { get; set; }
    public int? AvailableCapacity { get; set; }
    public int? BasePrice { get; set; }
    public TripStatus? TripStatus { get; set; }
}

/* Nitelikler hem nullable hem de Required olarak iþaretlendi. 
 * Nitelik nullable olmazsa, kullanýcý o deðeri göndermediði anda ASP ona 0 deðerini atar. Bu nedenle Required tag tetiklenmez.
 * Ancak nullable yaparsak, kullanýcý o deðeri göndermezse null olur ve Required tag tetiklenir.
 */

public class DashboardTripFilter
{
    [Required] public int? CompanyId { get; set; }
    public int? RouteId { get; set; }
    public int? VehicleId { get; set; }
    public int? DriverId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TripStatus? Status { get; set; } // Sadece iptal edilenleri vb. görmek isteyebilir
}


public class PassengerTripFilter
{
    [Required] public int? CompanyId { get; set; }
    [Required] public int? RouteId { get; set; }
    [Required] public TripDaySelection? DaySelection { get; set; }
}