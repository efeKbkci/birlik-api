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
public class PassengersController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPassengerById(int id)
    {
        var dto = await _context.Passengers
            .Where(p => p.Id == id)
            .ProjectTo<PassengerReadDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (dto == null)
            return NotFound($"Passenger with ID = {id} doesn't exist or deleted in DB.");

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePassenger(PassengerCreateDto dto)
    {
        var entity = _mapper.Map<Passenger>(dto);

        _context.Passengers.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPassengerById), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdatePassenger(int id, PassengerPatchDto dto)
    {
        var entity = await _context.Passengers.FindAsync(id);
        if (entity == null)
            return NotFound($"Passenger with ID = {id} doesn't exist in DB.");

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePassenger(int id)
    {
        var entity = await _context.Passengers.FindAsync(id);
        if (entity == null)
            return NotFound($"Passenger with ID = {id} doesn't exist in DB.");

        entity.IsDeleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestorePassenger(int id)
    {
        var entity = await _context.Passengers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null)
            return NotFound($"{id} numaralı yolcu veri tabanında yok.");

        if (!entity.IsDeleted)
            return BadRequest("Bu yolcu zaten silinmemiş.");

        entity.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Yolcu başarıyla kurtarıldı." });
    }
}
