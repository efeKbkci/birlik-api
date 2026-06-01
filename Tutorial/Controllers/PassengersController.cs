using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PassengersController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen yolcu bilgilerini getirir.
    /// </summary>
    /// <response code="200">Yolcu başarıyla bulundu. Detaylar döner.</response>
    /// <response code="404">Yolcu bulunamadı veya silinmiş.</response>
    [ProducesResponseType(typeof(PassengerReadDto), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Yeni bir yolcu oluşturur.
    /// </summary>
    /// <response code="201">Yolcu başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(PassengerReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreatePassenger(PassengerCreateDto dto)
    {
        var entity = _mapper.Map<Passenger>(dto);

        _context.Passengers.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPassengerById), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Mevcut yolcunun bilgilerini kısmen günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Yolcu bulunamadı.</response>
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

    /// <summary>
    /// Yolcuyu soft-delete (görünmez) yapar.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Yolcu bulunamadı.</response>
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

    /// <summary>
    /// Soft-deleted yolcuyu geri yükler.
    /// </summary>
    /// <response code="200">Yolcu başarıyla geri yüklendi.</response>
    /// <response code="404">Yolcu bulunamadı.</response>
    /// <response code="400">Yolcu zaten silinmemişse döner.</response>
    [ProducesResponseType(typeof(PassengerDeleteIncludedDto), StatusCodes.Status200OK)]
    [HttpPut("{id}/restore")]
    public async Task<IActionResult> RestorePassenger(int id)
    {
        var passenger = await _context.Passengers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (passenger == null)
            return NotFound();

        if (!passenger.IsDeleted)
            return BadRequest();

        passenger.IsDeleted = false;
        await _context.SaveChangesAsync();

        var passengerDto = _mapper.Map<PassengerDeleteIncludedDto>(passenger);
        return Ok(passengerDto);
    }
}
