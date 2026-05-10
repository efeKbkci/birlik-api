using Tutorial.Enums;

namespace Tutorial.Entities;

/*
 One to Many ilişkisine göre oluşturdum;   
    Route -> ICollection<Trip>

 Oluşturmadım:
    Company -> ICollection<Trip>
    Vehicle -> ICollection<Trip>
    Driver  -> ICollection<Trip>
*/
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
    public int AvailableCapacity { get; set; } 
    public TripStatus TripStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}
