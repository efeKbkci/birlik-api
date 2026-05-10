namespace Tutorial.Entities;

/*
 Company -> ICollection<Vehicle>
 */
public class Vehicle
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    public int? DefaultDriverId { get; set; }
    public Driver? DefaultDriver { get; set; }
    public string PlateNumber { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
