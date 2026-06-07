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
public class VehiclesController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen araç bilgisini getirir.
    /// </summary>
    /// <response code="200">Araç bulundu. Detaylar döner.</response>
    /// <response code="404">Araç bulunamadı veya görünür değil.</response>
    [ProducesResponseType(typeof(DetailedVehicleReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        var vehicleDto = await _context.Vehicles
                                .Where(v => v.Id == id)
                                .ProjectTo<DetailedVehicleReadDto>(_mapper.ConfigurationProvider)
                                .FirstOrDefaultAsync();

        if (vehicleDto == null)
        {
            return NotFound($"Vehicle with ID = {id} doesn't exist or inactive in DB.");
        }

        return Ok(vehicleDto);
    }

    [HttpGet("company/{companyId}/management-page")]
    public async Task<IActionResult> GetVehiclesManagementPage(int companyId)
    {
        var today = DateTime.UtcNow.Date; // UTC kullanmak her zaman daha güvenlidir

        // 1. Sadece CountAsync kullanarak RAM'e veri indirmeden sayıları alıyoruz
        var totalVehicles = await _context.Vehicles
            .CountAsync(v => v.CompanyId == companyId);

        var maintainedVehiclesCount = await _context.Vehicles
            .CountAsync(v => v.CompanyId == companyId && !v.IsActive);

        var underwayVehiclesCount = await _context.Trips
            .CountAsync(d => d.CompanyId == companyId && d.TripStatus == TripStatus.Underway);

        // 2. LİSTEYİ ÇEK (Grid'e basılacak araçlar)
        var vehiclesList = await _context.Vehicles
            .Where(v => v.CompanyId == companyId)
            .OrderByDescending(v => v.Id)
            .ProjectTo<VehicleListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        // 3. PAKETLE VE GÖNDER (Tek bir DTO içinde birleştiriyoruz)
        var pageData = new VehicleManagementPageDto
        {
            TotalVehicles = totalVehicles,
            MaintedVehicleCount = maintainedVehiclesCount,
            VehiclesOnTheRoadCount = underwayVehiclesCount,
            Vehicles = vehiclesList
        };

        return Ok(pageData); // 200 OK ile JSON olarak fırlat
    }

    /// <summary>
    /// Belirtilen firmaya ait araçların özet listesini döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Araç listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<BasicVehicleReadDto>), StatusCodes.Status200OK)]
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetVehiclesByCompany(int companyId)
    {
        var dtoList = await _context.Vehicles
            .Where(v => v.CompanyId == companyId)
            .ProjectTo<BasicVehicleReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

    /// <summary>
    /// Belirtilen firmaya ait silinmiş araçları döner (admin görünümü).
    /// </summary>
    /// <response code="200">İşlem başarılı. Silinmiş araçların listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<VehicleDeleteIncludedDto>), StatusCodes.Status200OK)]
    [HttpGet("company/{companyId}/deleted")]
    public async Task<IActionResult> GetDeletedVehiclesByCompany(int companyId)
    {
        var dtoList = await _context.Vehicles
            .IgnoreQueryFilters()
            .Where(v => v.CompanyId == companyId && v.IsDeleted)
            .ProjectTo<VehicleDeleteIncludedDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

    /// <summary>
    /// Yeni bir araç oluşturur.
    /// </summary>
    /// <response code="201">Araç başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(DetailedVehicleReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(VehicleCreateDto dto)
    {
        var newVehicle = _mapper.Map<Vehicle>(dto);

        _context.Vehicles.Add(newVehicle);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehicleById), new { id = newVehicle.Id }, newVehicle);
    }

    /// <summary>
    /// Belirtilen aracı tamamen günceller (replace/put davranışı).
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Araç bulunamadı.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> ReplaceVehicle(int id, [FromBody] VehicleCreateDto dto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        _mapper.Map(dto, vehicle);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Mevcut aracın alanlarını kısmen günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Araç bulunamadı.</response>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, VehiclePatchDto dto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return NotFound($"Vehicle with ID = {id} doesn't exist in DB.");
        }

        _mapper.Map(dto, vehicle);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Aracı soft-delete (görünmez) yapar.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Araç bulunamadı.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
            return NotFound($"Vehicle with ID = {id} doesn't exist in DB.");

        vehicle.IsDeleted = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Soft-deleted aracı geri yükler.
    /// </summary>
    /// <response code="200">Araç başarıyla geri yüklendi.</response>
    /// <response code="404">Araç bulunamadı.</response>
    /// <response code="400">Araç zaten silinmemişse döner.</response>
    [ProducesResponseType(typeof(VehicleDeleteIncludedDto), StatusCodes.Status200OK)]
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreVehicle(int id)
    {
        var vehicle = await _context.Vehicles
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
            return NotFound();

        if (!vehicle.IsDeleted)
            return BadRequest();

        vehicle.IsDeleted = false;
        await _context.SaveChangesAsync();

        var vehicleDto = _mapper.Map<VehicleDeleteIncludedDto>(vehicle);
        return Ok(vehicleDto);
    }
}
