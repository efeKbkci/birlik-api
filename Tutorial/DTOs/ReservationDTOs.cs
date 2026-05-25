using Tutorial.Enums;

namespace Tutorial.DTOs;

public class DetailedReservationReadDto
{
    public int Id { get; set; }
    public string RouteName { get; set; }
    public string PlateNumber { get; set; }
    public int BasePrice { get; set; }
    public DateTime DepartureTime { get; set; }
    public string PassengerName { get; set; }
    public string PassengerNumber { get; set; }
    public string PickupStopName { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
}

public class BasicReservationReadDto
{
    public int Id { get; set; }
    public string RouteName { get; set; }
    public string PassengerName { get; set; }
    public string PassengerNumber { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
}

public class ReservationCreateDto
{
    public int TripId { get; set; }
    public int PassengerId { get; set; }
    public int PickupStopId { get; set; }
    public ReservationStatus ReservationStatus { get; set; }
    public PassengerStatus PassengerStatus { get; set; }
}

public class ReservationPatchDto
{
    // PassengerId değiştirilemez. 
    public int? TripId { get; set; }
    public int? PickupStopId { get; set; }
    public ReservationStatus? ReservationStatus { get; set; }
    public PassengerStatus? PassengerStatus { get; set; }
}
