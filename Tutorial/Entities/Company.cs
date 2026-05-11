namespace Tutorial.Entities;

public class Company
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string ContactPhone { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false; // Veri silme işleminin kontrol edildiği sütun
    public DateTime CreatedAt { get; set; }
    public ICollection<Driver> Drivers { get; set; }
    public ICollection<Vehicle> Vehicles { get; set; }
}
