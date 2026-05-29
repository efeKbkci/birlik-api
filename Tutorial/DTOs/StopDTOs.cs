namespace Tutorial.DTOs;


public class DetailedStopReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string RouteName { get; set; }
    public string StopName { get; set; }
    public int StopOrder { get; set; }
    public int? TimeOffsetMins { get; set; }
    public bool IsActive { get; set; }
}

public class BasicStopReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; }
    public string StopName { get; set; }
    public bool IsActive { get; set; }
}

public class StopDeleteIncludedDto : BasicStopReadDto
{
    public bool IsDeleted { get; set; }
}

public class StopCreateDto
{
    public int CompanyId { get; set; }
    public int RouteId { get; set; }
    public string StopName { get; set; }
    public int StopOrder { get; set; }
    public int? TimeOffsetMins { get; set; }
    public bool IsActive { get; set; }
}

public class StopPatchDto
{
    public int? CompanyId { get; set; }
    public int? RouteId { get; set; }
    public string? StopName { get; set; }
    public int? StopOrder { get; set; }
    public int? TimeOffsetMins { get; set; }
    public bool? IsActive { get; set; }
}
