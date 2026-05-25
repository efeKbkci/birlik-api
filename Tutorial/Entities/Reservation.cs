using Tutorial.Enums;

namespace Tutorial.Entities;

/*
 One to Many ilişkisine göre oluşturdum;   
    Passenger -> ICollection<Reservation>
    Trip      -> ICollection<Reservation>

 Oluşturmadım:
    Stop      -> ICollection<Reservation> 

 Bir yolcu geçmiş veya gelecek seyahatlerini sorgulayabilir. 
 Bir sefere ait reservasyonlar sorgulanabilir. 
 Bir durağa ait reservasyonlar için doğrudan böyle bir sorguya gerek yoktur. 
 */
public class Reservation 
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public Trip Trip { get; set; }
    public int PassengerId { get; set; }
    public Passenger Passenger { get; set; }
    public int PickupStopId { get; set; }
    public Stop PickupStop { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
