namespace Tutorial.DTOs;

public class DetailedDriverReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public bool IsActive { get; set; }
}

public class BasicDriverReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class DriverDeleteIncludedDto : BasicDriverReadDto {
    public bool IsDeleted { get; set; }
}

public class DriverCreateDto
{
    public int CompanyId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; } // Bir şoför sisteme eklendiği anda çalışmaya başlamayabilir.
}

public class DriverPatchDto 
{
    public int? CompanyId { get; set; } // Şoförün çalıştığı firma değişebilir.
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PasswordHash { get; set; }
    public bool? IsActive { get; set; }
}


