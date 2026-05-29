using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CompaniesController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    // Endpoint: GET api/companies/5
    // Buradaki "{id}" ifadesi URL'den gelecek dinamik bir sayıdır.
    // İçindeki isim ile metodun parametresindeki (int id) ismi aynı olmalıdır.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompanyById(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound($"Firm with ID = {id} doesn't exist in DB.");
        }

        // _mapper.Map<HedefTip>(KaynakVeri) 
        var companyDto = _mapper.Map<DetailedCompanyReadDto>(company);

        return Ok(companyDto);
    }

    // Kullanım (Silinenler Dahil): GET api/companies?includeDeleted=true
    [HttpGet]
    public async Task<IActionResult> GetCompanies([FromQuery] bool includeDeleted = false)
    {
        // Sorguyu modifiye edebileceğimiz şekilde oluşturuyoruz.
        var query = _context.Companies.AsQueryable();

        // Eğer kullanıcı silinenleri de istiyorsa, filtreyi devre dışı bırak.
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        var companies = await query.ToListAsync();

        if (includeDeleted)
        {
            var adminResult = _mapper.Map<List<CompanyDeleteIncludedDto>>(companies);
            return Ok(adminResult);
        }

        var standardResult = _mapper.Map<List<BasicCompanyReadDto>>(companies);
        return Ok(standardResult);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCompany(CompanyCreateDto dto)
    {
        // Business Logic: Veri tabanına aynı iletişim numarasına sahip 2 firma ekleme.
        bool isPhoneExist = await _context.Companies.AnyAsync(c => c.ContactPhone == dto.ContactPhone);

        if (isPhoneExist)
        {
            // 409 Conflict durumu, "Bu veri zaten var ve çakışıyor" demektir.        
            return Conflict($"'{dto.ContactPhone}' telefon numarasına sahip aktif bir firma zaten sistemde kayıtlı.");
        }

        var newCompany = _mapper.Map<Company>(dto);

        _context.Companies.Add(newCompany);

        await _context.SaveChangesAsync();

        // REST Standartı: Yeni oluşan kaydı ve 201 Created kodunu dön
        return CreatedAtAction(nameof(GetCompanyById), new { id = newCompany.Id }, newCompany);
    }

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
        _mapper.Map(dto, company);

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
        company.IsDeleted = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreCompany(int id)
    {
        // IgnoreQueryFilters() ile OnModelCreating metodu altında eklediğimiz filtreyi deliyoruz.
        // Not: FindAsync ile IgnoreQueryFilters doğrudan yan yana kullanılamaz, 
        // bu yüzden FirstOrDefaultAsync kullanıyoruz.
        var company = await _context.Companies
                                    .IgnoreQueryFilters()
                                    .FirstOrDefaultAsync(c => c.Id == id);

        if (company == null)
            return NotFound($"{id} numaralı firma veri tabanında yok.");

        if (!company.IsDeleted)
            return BadRequest("Bu firma zaten silinmemiş.");

        company.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Firma başarıyla kurtarıldı." });
    }
}

 
