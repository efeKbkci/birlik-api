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
public class VehiclesController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

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

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetVehiclesByCompany(int companyId)
    {
        var dtoList = await _context.Vehicles
            .Where(v => v.CompanyId == companyId)
            .ProjectTo<BasicVehicleReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateVehicle(VehicleCreateDto dto)
    {
        var newVehicle = _mapper.Map<Vehicle>(dto);

        _context.Vehicles.Add(newVehicle);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehicleById), new { id = newVehicle.Id }, newVehicle);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ReplaceVehicle(int id, [FromBody] VehicleCreateDto dto)
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

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreVehicle(int id)
    {
        var vehicle = await _context.Vehicles
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
            return NotFound($"{id} numaralı araç veri tabanında yok.");

        if (!vehicle.IsDeleted)
            return BadRequest("Bu araç zaten silinmemiş.");

        vehicle.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Araç başarıyla kurtarıldı." });
    }
}
