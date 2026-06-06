namespace Tutorial.Entities;

public class Admin
{
    public int Id { get; set; }
    public int CompanyId { get; set; } // Foreign Key
    public Company Company { get; set; } // Navigation Property
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? LoginAt { get; set; } // İlk kayıtta null olabilir
    public DateTime CreatedAt { get; set; }
}