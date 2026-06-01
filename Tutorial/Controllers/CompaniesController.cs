using AutoMapper;
using AutoMapper.QueryableExtensions;
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

    /// <summary>
    /// ID ile eşleşen şirket bilgisini getirir.
    /// </summary>
    /// <response code="200">Şirket başarıyla bulundu. Yanıt gövdesinde şirket detayları yer alır.</response>
    /// <response code="404">Şirket bulunamadı veya silinmiş.</response>
    [ProducesResponseType(typeof(DetailedCompanyReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompanyById(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
        {
            return NotFound();
        }

        // _mapper.Map<HedefTip>(KaynakVeri) 
        var companyDto = _mapper.Map<DetailedCompanyReadDto>(company);

        return Ok(companyDto);
    }

    /// <summary>
    /// Tüm şirketleri döner. Eğer includeDeleted=true verilirse silinmiş kayıtlar da alınır.
    /// </summary>
    /// <response code="200">İşlem başarılı. Şirket listesi döner. (Standart kullanıcılar için özet, includeDeleted=true ile silinmiş kayıtlar dahil admin görünümü)</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BasicCompanyReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<CompanyDeleteIncludedDto>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Yeni bir şirket oluşturur.
    /// </summary>
    /// <response code="201">Şirket başarıyla oluşturuldu. Oluşturulan şirket nesnesi döner.</response>
    /// <response code="409">Aynı telefon numarasıyla çakışma durumunda döner.</response>
    [ProducesResponseType(typeof(DetailedCompanyReadDto), StatusCodes.Status201Created)]
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

    /// <summary>
    /// Belirtilen şirketin alanlarını günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">şirket bulunamadı.</response>
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

    /// <summary>
    /// Şirketi soft-delete yöntemi ile siler.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Şirket bulunamadı.</response>
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

    /// <summary>
    /// Soft-deleted olan şirketi geri yükler.
    /// </summary>
    /// <response code="200">Şirket başarıyla geri yüklendi.</response>
    /// <response code="404">Şirket bulunamadı.</response>
    /// <response code="400">Şirket zaten silinmemişse döner.</response>
    [ProducesResponseType(typeof(CompanyDeleteIncludedDto), StatusCodes.Status200OK)]
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
            return NotFound();

        if (!company.IsDeleted)
            return BadRequest();

        company.IsDeleted = false;
        await _context.SaveChangesAsync();

        var companyDto = _mapper.Map<CompanyDeleteIncludedDto>(company);    
        return Ok(companyDto);
    }
}

 
