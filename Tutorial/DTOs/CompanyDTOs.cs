namespace Tutorial.DTOs;

public class CompanyReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string ContactPhone { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
}

public class CompanyCreateDto
{
    public string CompanyName { get; set; }
    public string ContactPhone { get; set; }
    public string Location { get; set; }
}

public class CompanyPatchDto
{
    public string? CompanyName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Location { get; set; }
    public bool? IsActive { get; set; }
}
