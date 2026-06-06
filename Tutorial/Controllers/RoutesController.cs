using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Birlik.Shared.DTOs;
using Tutorial.Entities;
using Route = Tutorial.Entities.Route;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoutesController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen güzergah bilgisini getirir.
    /// </summary>
    /// <response code="200">Güzergah bulundu. Detaylar döner.</response>
    /// <response code="404">Güzergah bulunamadı.</response>
    [ProducesResponseType(typeof(RouteReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(int id)
    {
        var routeDto = await _context.Routes
                        .Where(d => d.Id == id)
                        .ProjectTo<RouteReadDto>(_mapper.ConfigurationProvider)
                        .FirstOrDefaultAsync();

        if (routeDto == null)
            return NotFound();

        return Ok(routeDto);
    }

    /// <summary>
    /// Yeni bir güzergah oluşturur.
    /// </summary>
    /// <response code="201">Güzergah başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(RouteReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateRoute(RouteCreateDto dto)
    {
        var newRoute = _mapper.Map<Route>(dto);

        _context.Routes.Add(newRoute);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRouteById), new { id = newRoute.Id }, newRoute);
    }

    /// <summary>
    /// Belirtilen güzergahın verilerini kısmen günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Güzergah bulunamadı.</response>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateRoute(int id, RoutePatchDto dto)
    {
        var route = await _context.Routes.FindAsync(id);

        if (route == null)
        {
            return NotFound($"Route with ID = {id} doesn't exist in DB.");
        }

        _mapper.Map(dto, route);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Güzergahı soft-delete (görünmez) yapar.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Güzergah bulunamadı.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(int id)
    {
        var route = await _context.Routes.FindAsync(id);

        if (route == null)
            return NotFound($"Route with ID = {id} doesn't exist in DB.");

        route.IsDeleted = true; // Soft Delete işlemi ile veriyi görünmez yapıyoruz.

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Soft-deleted güzergahı geri yükler.
    /// </summary>
    /// <response code="200">Güzergah başarıyla geri yüklendi.</response>
    /// <response code="404">Güzergah bulunamadı.</response>
    /// <response code="400">Güzergah zaten silinmemişse döner.</response>
    [ProducesResponseType(typeof(RouteDeleteIncludedDto), StatusCodes.Status200OK)]
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreRoute(int id)
    {
        var route = await _context.Routes
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(c => c.Id == id);

        if (route == null)
            return NotFound();

        if (!route.IsDeleted)
            return BadRequest();

        route.IsDeleted = false;
        await _context.SaveChangesAsync();

        var routeDto = _mapper.Map<RouteDeleteIncludedDto>(route);
        return Ok(routeDto);
    }
}
