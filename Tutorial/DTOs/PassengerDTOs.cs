namespace Tutorial.DTOs;

public class PassengerReadDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class PassengerDeleteIncludedDto : PassengerReadDto
{
    public bool IsDeleted { get; set; }
}

public class PassengerCreateDto
{
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class PassengerPatchDto
{
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
