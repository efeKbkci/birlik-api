namespace Tutorial.DTOs;

public class CityReadDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CityDeleteIncludedDto : CityReadDto
{
    public bool IsDeleted { get; set; }
}

public class CityCreateDto
{
    public string Name { get; set; }
}

public class CityPatchDto
{
    public string Name { get; set; }
}


