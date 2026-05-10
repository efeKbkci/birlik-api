using System.ComponentModel.DataAnnotations.Schema;

namespace Tutorial.Entities;

public class Route
{
    public int Id { get; set; } // PK
    public int CompanyId { get; set; } // FK
    public Company Company { get; set; } // Navigation Property
    public int DepartureCityId { get; set; } // FK
    public City DepartureCity { get; set; } // Navigation Property
    public int ArrivalCityId { get; set; } // FK
    public City ArrivalCity { get; set; } // Navigation Property
    public int EstimatedDuration { get; set; }
    public int BasePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public ICollection<Stop> Stops { get; set; }
    public ICollection<Trip> Trips { get; set; }
}
