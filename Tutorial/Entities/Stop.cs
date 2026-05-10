namespace Tutorial.Entities;

public class Stop
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public string StopName { get; set; }
    public int StopOrder { get; set; }
    public int? TimeOffsetMins { get; set; }
    public bool IsActive { get; set; }
}
