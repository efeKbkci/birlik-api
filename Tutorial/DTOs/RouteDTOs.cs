namespace Tutorial.DTOs
{
    public class RouteReadDto
    {
        public int Id { get; set; }
        public string DepartureCityName { get; set; }
        public string ArrivalCityName { get; set; } 
        public int EstimatedDuration { get; set; }
    }

    public class RouteDeleteIncludedDto : RouteReadDto 
    { 
        public bool IsDeleted { get; set; }
    }

    public class RouteCreateDto
    {
        public int DepartureCityId { get; set; } // FK
        public int ArrivalCityId { get; set; } // FK
        public int EstimatedDuration { get; set; }
    }

    public class RoutePatchDto
    {
        public int? DepartureCityId { get; set; } // FK
        public int? ArrivalCityId { get; set; } // FK
        public int? EstimatedDuration { get; set; }
    }
}
