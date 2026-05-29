namespace Tutorial.DTOs;

public class DetailedVehicleReadDto {
    public int Id { get; set; }
    public string CompanyName { get; set; } 
    public string DefaultDriverName { get; set; }
    public string PlateNumber { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class BasicVehicleReadDto {
    public int Id { get; set; }
    public string PlateNumber { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class VehicleDeleteIncludedDto : BasicVehicleReadDto {
    public bool IsDeleted { get; set; }
}

public class VehicleCreateDto {
    public int CompanyId { get; set; }
    public int? DefaultDriverId { get; set; }
    public string PlateNumber { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class VehiclePatchDto {
    public int? CompanyId { get; set; }
    public int? DefaultDriverId { get; set; }
    public string? PlateNumber { get; set; }
    public int? Capacity { get; set; }
    public bool? IsActive { get; set; }
}
