using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Birlik.Shared.DTOs;
using Tutorial.Entities;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen rezervasyonun detaylarını getirir.
    /// </summary>
    /// <response code="200">Rezervasyon bulundu. Detaylar döner.</response>
    /// <response code="404">Rezervasyon bulunamadı veya silinmiş.</response>
    [ProducesResponseType(typeof(DetailedReservationReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservationById(int id)
    {
        var dto = await _context.Reservations
            .Where(r => r.Id == id)
            .ProjectTo<DetailedReservationReadDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (dto == null)
            return NotFound($"Reservation with ID = {id} doesn't exist or deleted in DB.");

        return Ok(dto);
    }

    /// <summary>
    /// Belirli bir sefere ait rezervasyonların özet listesini döner.
    /// </summary>
    /// <response code="200">İşlem başarılı. Rezervasyon listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<BasicReservationReadDto>), StatusCodes.Status200OK)]
    [HttpGet("trip/{tripId}")]
    public async Task<IActionResult> GetReservationsByTrip(int tripId)
    {
        var list = await _context.Reservations
            .Where(r => r.TripId == tripId)
            .ProjectTo<BasicReservationReadDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Yeni bir rezervasyon oluşturur.
    /// </summary>
    /// <response code="201">Rezervasyon başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    [ProducesResponseType(typeof(DetailedReservationReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateReservation(ReservationCreateDto dto)
    {
        var entity = _mapper.Map<Reservation>(dto);

        _context.Reservations.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservationById), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Mevcut rezervasyonun alanlarını günceller.
    /// </summary>
    /// <response code="204">Güncelleme başarılı.</response>
    /// <response code="404">Rezervasyon bulunamadı.</response>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, ReservationPatchDto dto)
    {
        var entity = await _context.Reservations.FindAsync(id);
        if (entity == null)
            return NotFound($"Reservation with ID = {id} doesn't exist in DB.");

        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
