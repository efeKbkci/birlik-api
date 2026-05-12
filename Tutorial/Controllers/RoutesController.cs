using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;
using Route = Tutorial.Entities.Route;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoutesController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

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

    [HttpPost]
    public async Task<IActionResult> CreateRoute(RouteCreateDto dto)
    {
        var newRoute = _mapper.Map<Route>(dto);

        _context.Routes.Add(newRoute);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRouteById), new { id = newRoute.Id }, newRoute);
    }

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

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreRoute(int id)
    {
        var route = await _context.Routes
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(c => c.Id == id);

        if (route == null)
            return NotFound($"{id} numaralı güzergah veri tabanında yok.");

        if (!route.IsDeleted)
            return BadRequest("Bu güzergah zaten silinmemiş.");

        route.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Güzergah başarıyla kurtarıldı." });
    }
}
