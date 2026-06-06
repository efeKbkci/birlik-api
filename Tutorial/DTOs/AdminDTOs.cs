namespace Tutorial.DTOs;

public class AdminReadDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } // AutoMapper bunu halledecek
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime? LoginAt { get; set; }
}

public class AdminCreateDto
{
    public int CompanyId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; } // İstek atarken hash'li değil, düz şifre gelir
}

public class AdminLoginDto
{
    public string EmailOrPhone { get; set; }
    public string Password { get; set; }
}