using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompaniesController(AppDbContext context) : ControllerBase
{
    private readonly AppDbContext _context = context;

    // Kullanım (Silinenler Dahil): GET api/companies?includeInactives=true
    [HttpGet]
    public async Task<IActionResult> GetCompanies([FromQuery] bool includeInactives = false)
    {
        // Sorguyu modifiye edebileceğimiz şekilde oluşturuyoruz.
        var query = _context.Companies.AsQueryable();

        // Eğer kullanıcı silinenleri de istiyorsa, filtreyi devre dışı bırak.
        if (includeInactives)
        {
            query = query.IgnoreQueryFilters();
        }

        var companies = await query.ToListAsync();

        var dtoList = companies.Select(c => new CompanyReadDto 
            { 
                Id = c.Id,
                CompanyName = c.CompanyName,
                ContactPhone = c.ContactPhone,
                Location = c.Location,
                IsActive = c.IsActive
            }
        );

        return Ok(dtoList);
    }

    // Endpoint: GET api/companies/5
    // Buradaki "{id}" ifadesi URL'den gelecek dinamik bir sayıdır.
    // İçindeki isim ile metodun parametresindeki (int id) ismi aynı olmalıdır.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound($"Firm with ID = {id} doesn't exist in DB.");
        }

        var companyDto = new CompanyReadDto
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            ContactPhone = company.ContactPhone,
            Location = company.Location,
            IsActive = company.IsActive
        };

        return Ok(companyDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany(CompanyCreateDto dto)
    {
        var newCompany = new Company
        {
            CompanyName = dto.CompanyName,
            ContactPhone = dto.ContactPhone,
            Location = dto.Location,
            IsActive = true, 
            CreatedAt = DateTime.UtcNow 
        };

        _context.Companies.Add(newCompany);

        await _context.SaveChangesAsync();

        // REST Standartı: Yeni oluşan kaydı ve 201 Created kodunu dön
        return CreatedAtAction(nameof(GetCompany), new { id = newCompany.Id }, newCompany);
    }

    [HttpPut("{id}/reactivate")]
    public async Task<IActionResult> ReactivateCompany(int id)
    {
        // IgnoreQueryFilters() ile OnModelCreating metodu altında eklediğimiz filtreyi deliyoruz.
        // Not: FindAsync ile IgnoreQueryFilters doğrudan yan yana kullanılamaz, 
        // bu yüzden FirstOrDefaultAsync kullanıyoruz.
        var company = await _context.Companies
                                    .IgnoreQueryFilters()
                                    .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound($"{id} numaralı firma veri tabanında yok.");

        if (company.IsActive)
            return BadRequest("Bu firma zaten aktif durumda.");

        company.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Firma başarıyla tekrar aktif edildi." });
    }

    /* Bu metot bir şirketi pasif hale getirmek için kullanılabilir fakat
     * Pasif bir şirketi aktif hale getirmek için kullanılamaz. 
     * Çünkü pasif şirketler bu metot içerisinde listelenmez. 
     * Pasif bir şirketi güncelleyebilmek için önce şirketi aktif hale getirmeliyiz.
     */
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateCompany(int id, CompanyPatchDto dto)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound($"Firm with ID = {id} doesn't exist in DB.");
        }

        /* Kullanıcı JSON verisinde hangi niteliği değiştirmek istiyorsa
         * yalnızca o niteliğin güncel değerini gönderir. Bu durumda diğer nitelikler null olur. 
         * null olan değerler güncellenmez. 
         */
        if (dto.CompanyName != null) company.CompanyName = dto.CompanyName;
        if (dto.ContactPhone != null) company.ContactPhone = dto.ContactPhone;
        if (dto.Location != null) company.Location = dto.Location;
        if (dto.IsActive != null) company.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound($"Firm with ID = {id} doesn't exist in DB.");

        // Soft Delete işlemi ile veriyi görünmez yapıyoruz.
        company.IsActive = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

 
