namespace Tutorial.Entities;

public class Passenger
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Reservation> Reservations { get; set; }
}
