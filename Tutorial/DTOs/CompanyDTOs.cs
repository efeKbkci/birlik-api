namespace Tutorial.DTOs;

public class DetailedCompanyReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string ContactPhone { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
}

public class BasicCompanyReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public bool IsActive { get; set; }
}

public class CompanyDeleteIncludedDto : BasicCompanyReadDto 
{
    public bool IsDeleted { get; set; }
}

public class CompanyCreateDto
{
    public string CompanyName { get; set; }
    public string ContactPhone { get; set; }
    public string Location { get; set; }
    public bool IsActive { get; set; }
}

public class CompanyPatchDto
{
    public string? CompanyName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Location { get; set; }
    public bool? IsActive { get; set; }
}
