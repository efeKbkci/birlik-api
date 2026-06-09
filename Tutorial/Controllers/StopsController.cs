using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Tutorial.Entities;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StopsController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen durak bilgisini getirir.
    /// </summary>
    /// <response code="200">Durak bulundu. Detaylar döner.</response>
    /// <response code="404">Durak bulunamadı veya silinmiş.</response>
    [ProducesResponseType(typeof(DetailedStopReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStopById(int id)
    {
        var dto = await _context.Stops
            .Where(s => s.Id == id)
            .ProjectTo<DetailedStopReadDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (dto == null)
            return NotFound($"Stop with ID = {id} doesn't exist or deleted in DB.");

        return Ok(dto);
    }

    /// <summary>
    /// Belirtilen şirket ve güzergaha ait durakların özet listesini döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Durak listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<BasicStopReadDto>), StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> GetStops([FromQuery] int companyId, [FromQuery] int routeId)
    {
        var list = await _context.Stops
            .Where(s => s.CompanyId == companyId && s.RouteId == routeId)
            .OrderBy(s => s.StopOrder)
            .ProjectTo<BasicStopReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("company/{companyId}/management-page")]
    public async Task<IActionResult> GetStopManagementPage(int companyId)
    {
        var stopsList = await _context.Stops
            .Where(s => s.CompanyId == companyId && !s.IsDeleted)
            .OrderBy(s => s.StopOrder)
            .ProjectTo<StopListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pageData = new StopManagementPageDto
        {
            Stops = stopsList
        };

        return Ok(pageData); 
    }

    /// <summary>
    /// Belirtilen firmaya ait silinmiş durakları döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Silinmiş durakların listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<StopDeleteIncludedDto>), StatusCodes.Status200OK)]
    [HttpGet("company/{companyId}/deleted")]
    public async Task<IActionResult> GetDeletedStopsByCompany(int companyId)
    {
        var list = await _context.Stops
            .IgnoreQueryFilters()
            .Where(s => s.CompanyId == companyId && s.IsDeleted)
            .ProjectTo<StopDeleteIncludedDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Yeni bir durak oluşturur.
    /// </summary>
    /// <response code="201">Durak başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(DetailedStopReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateStop(StopCreateDto dto)
    {
        var entity = _mapper.Map<Stop>(dto);

        _context.Stops.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStopById), new { id = entity.Id }, entity);
    }

    /// <summary> 
    /// Mevcut durak bilgisini günceller. 
    /// </summary> 
    /// <response code="204">Güncelleme başarılı.</response> 
    /// <response code="404">Durak bulunamadı.</response> 
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStop(int id, StopPatchDto dto)
    {
        var entity = await _context.Stops.FindAsync(id);
        if (entity == null)
            return NotFound();

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary> 
    /// Durakı soft-delete (görünmez) yapar.
    /// </summary> 
    /// <response code="204">Silme başarılı.</response> 
    /// <response code="404">Durak bulunamadı.</response> 
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStop(int id)
    {
        var entity = await _context.Stops.FindAsync(id);
        if (entity == null)
            return NotFound();

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Soft-deleted durakı geri yükler.
    /// </summary>
    /// <response code="200">Durak başarıyla geri yüklendi. Bilgilendirme mesajı döner.</response>
    /// <response code="404">Durak bulunamadı.</response>
    /// <response code="400">Durak zaten silinmemişse döner.</response>
    [ProducesResponseType(typeof(StopDeleteIncludedDto), StatusCodes.Status200OK)]
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreStop(int id)
    {
        var stop = await _context.Stops
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stop == null)
            return NotFound();

        if (!stop.IsDeleted)
            return BadRequest();

        stop.IsDeleted = false;
        await _context.SaveChangesAsync();

        var stopDto = _mapper.Map<StopDeleteIncludedDto>(stop); 
        return Ok(stopDto);
    }
}
