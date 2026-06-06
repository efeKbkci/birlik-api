using Birlik.Shared.Enums;

namespace Tutorial.Entities;

public class Trip
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public int RouteId { get; set; }
    public Route Route { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
    public int DriverId { get; set; }
    public Driver Driver { get; set; }  
    public DateTime DepartureTime { get; set; }
    public int Capacity { get; set; }
    public int PassengerNumbers { get; set; }
    public int BasePrice { get; set; }
    public TripStatus TripStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    // IsDeleted sütunu kaldırıldı. Sefer silinemez, iptal edilebilir. 
    public ICollection<Reservation> Reservations { get; set; }
}
