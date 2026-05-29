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
public class DriversController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDriverById(int id)
    {
        var driverDto = await _context.Drivers
                                .Where(d => d.Id == id)
                                .ProjectTo<DetailedDriverReadDto>(_mapper.ConfigurationProvider)
                                .FirstOrDefaultAsync();
                                
        if (driverDto == null)
        {
            return NotFound($"Driver with ID = {id} doesn't exist or inactive in DB.");
        }

        return Ok(driverDto);
    }

    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetDriversByCompany(int companyId)
    {
        var dtoList = await _context.Drivers
            .Where(d => d.CompanyId == companyId)
            .ProjectTo<BasicDriverReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(dtoList);
    }

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

    [HttpPost]
    public async Task<IActionResult> CreateDriver(DriverCreateDto dto)
    {
        var newDriver = _mapper.Map<Driver>(dto);

        _context.Drivers.Add(newDriver);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDriverById), new { id = newDriver.Id }, newDriver);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateDriver(int id, DriverPatchDto dto) 
    {
        var driver = await _context.Drivers.FindAsync(id);

        if (driver == null)
        {
            return NotFound($"Driver with ID = {id} doesn't exist in DB.");
        }

        _mapper.Map(dto, driver);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDriver(int id) 
    {
        var driver = await _context.Drivers.FindAsync(id);

        if (driver == null)
            return NotFound($"Driver with ID = {id} doesn't exist in DB.");

        driver.IsDeleted = true; // Soft Delete işlemi ile veriyi görünmez yapıyoruz.

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestoreDriver(int id) 
    {
        var driver = await _context.Drivers
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(c => c.Id == id);

        if (driver == null)
            return NotFound($"{id} numaralı sürücü veri tabanında yok.");

        if (!driver.IsDeleted)
            return BadRequest("Bu sürücü zaten silinmemiş.");

        driver.IsDeleted = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Sürücü başarıyla kurtarıldı." });
    }
}
