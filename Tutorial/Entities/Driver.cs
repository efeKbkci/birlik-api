using Birlik.Shared.Enums;

namespace Tutorial.Entities;

public class Driver
{
    public int Id { get; set; } 
    public int CompanyId { get; set; } // Foreign Key
    public Company Company { get; set; } // Navigation Property
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DriverStatus Status { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } 
}
