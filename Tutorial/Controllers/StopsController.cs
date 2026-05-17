using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StopsController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetStopById(int id)
    {
        var dto = await _context.Stops
            .Where(s => s.Id == id)
            .ProjectTo<StopReadDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (dto == null)
            return NotFound($"Stop with ID = {id} doesn't exist or deleted in DB.");

        return Ok(dto);
    }

    [HttpGet]
    public async Task<IActionResult> GetStops([FromQuery] int companyId, [FromQuery] int routeId)
    {
        var list = await _context.Stops
            .Where(s => s.CompanyId == companyId && s.RouteId == routeId)
            .OrderBy(s => s.StopOrder)
            .ProjectTo<StopReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(list);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateStop(StopCreateDto dto)
    {
        var entity = _mapper.Map<Stop>(dto);

        _context.Stops.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetStopById), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStop(int id, StopPatchDto dto)
    {
        var entity = await _context.Stops.FindAsync(id);
        if (entity == null)
            return NotFound($"Stop with ID = {id} doesn't exist in DB.");

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStop(int id)
    {
        var entity = await _context.Stops.FindAsync(id);
        if (entity == null)
            return NotFound($"Stop with ID = {id} doesn't exist in DB.");

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreStop(int id)
    {
        var entity = await _context.Stops
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (entity == null)
            return NotFound($"{id} numaralý durak veri tabanýnda yok.");

        if (!entity.IsDeleted)
            return BadRequest("Bu durak zaten silinmemiþ.");

        entity.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Durak baþarýyla kurtarýldý." });
    }
}
