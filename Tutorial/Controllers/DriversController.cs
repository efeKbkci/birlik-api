using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Tutorial.Entities;
using Birlik.Shared.Enums;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DriversController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen sürücü bilgilerini getirir.
    /// </summary>
    /// <response code="200">İşlem başarılı. Sürücü bilgilerini döner.</response>
    /// <response code="404">Sürücü bulunamadı.</response>
    [ProducesResponseType(typeof(DetailedDriverReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriverById(int id)
    {
        var driverDto = await _context.Drivers
                                .Where(d => d.Id == id)
                                .ProjectTo<DetailedDriverReadDto>(_mapper.ConfigurationProvider)
                                .FirstOrDefaultAsync();
                                
        if (driverDto == null)
        {
            return NotFound();
        }

        return Ok(driverDto);
    }

    /// <summary>
    /// Belirtilen firmaya ait sürücülerin özet listesini döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Sürücü listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<BasicDriverReadDto>), StatusCodes.Status200OK)]
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetDriversByCompany(int companyId)
    {
        var dtoList = await _context.Drivers
            .Where(d => d.CompanyId == companyId)
            .ProjectTo<BasicDriverReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

    [HttpGet("company/{companyId}/management-page")]
    public async Task<IActionResult> GetDriversManagementPage(int companyId)
    {
        var today = DateTime.UtcNow.Date; // UTC kullanmak her zaman daha güvenlidir

        // 1. Sadece CountAsync kullanarak RAM'e veri indirmeden sayıları alıyoruz
        var totalDrivers = await _context.Drivers
            .CountAsync(d => d.CompanyId == companyId && !d.IsDeleted);

        var activeDriversCount = await _context.Drivers
            .CountAsync(d => d.CompanyId == companyId && d.Status != DriverStatus.Available);

        var activeVehiclesCount = await _context.Trips
            .CountAsync(t => t.CompanyId == companyId && t.TripStatus == TripStatus.Underway);

        // 2. LİSTEYİ ÇEK (Grid'e basılacak sürücüler)
        var driversList = await _context.Drivers
            .Where(d => d.CompanyId == companyId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ProjectTo<DriverListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        // 3. PAKETLE VE GÖNDER (Tek bir DTO içinde birleştiriyoruz)
        var pageData = new DriverManagementPageDto
        {
            TotalDrivers = totalDrivers,
            InActiveDriverCount = activeDriversCount,
            VehiclesOnTheRoadCount = activeVehiclesCount,
            Drivers = driversList
        };

        return Ok(pageData); // 200 OK ile JSON olarak fırlat
    }

    /// <summary>
    /// Belirtilen firmaya ait silinmiş sürücüleri döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Silinmiş sürücülerin listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<DriverDeleteIncludedDto>), StatusCodes.Status200OK)]
    [HttpGet("company/{companyId}/deleted")]
    public async Task<IActionResult> GetDeletedDriversByCompany(int companyId)
    {
        var dtoList = await _context.Drivers
            .IgnoreQueryFilters()
            .Where(d => d.CompanyId == companyId && d.IsDeleted) 
            .ProjectTo<DriverDeleteIncludedDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

    /// <summary>
    /// Yeni bir sürücü oluşturur.
    /// </summary>
    /// <response code="201">Sürücü başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(DetailedDriverReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateDriver(DriverCreateDto dto)
    {
        var newDriver = _mapper.Map<Driver>(dto);

        _context.Drivers.Add(newDriver);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDriverById), new { id = newDriver.Id }, newDriver);
    }

    /// <summary>
    /// Mevcut sürücünün bilgilerini günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Sürücü bulunamadı.</response>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDriver(int id, DriverPatchDto dto) 
    {
        var driver = await _context.Drivers.FindAsync(id);

        if (driver == null)
        {
            return NotFound();
        }

        _mapper.Map(dto, driver);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Sürücüyü soft-delete ile görünmez yapar.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Sürücü bulunamadı.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(int id) 
    {
        var driver = await _context.Drivers.FindAsync(id);

        if (driver == null)
            return NotFound();

        driver.IsDeleted = true; // Soft Delete işlemi ile veriyi görünmez yapıyoruz.

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Soft-deleted sürücüyü geri yükler.
    /// </summary>
    /// <response code="200">Sürücü başarıyla geri yüklendi.</response>
    /// <response code="404">Sürücü bulunamadı.</response>
    /// <response code="400">Sürücü zaten aktif.</response>
    [ProducesResponseType(typeof(DriverDeleteIncludedDto), StatusCodes.Status200OK)]
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreDriver(int id) 
    {
        var driver = await _context.Drivers
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(c => c.Id == id);

        if (driver == null)
            return NotFound();

        if (!driver.IsDeleted)
            return BadRequest();

        driver.IsDeleted = false;
        await _context.SaveChangesAsync();

        var driverDto = _mapper.Map<DriverDeleteIncludedDto>(driver);
        return Ok(driverDto);
    }
}
