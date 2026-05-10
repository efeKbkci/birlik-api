namespace Tutorial.Entities;

public class Driver
{
    public int Id { get; set; } 
    public int CompanyId { get; set; } // Foreign Key
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    
}
