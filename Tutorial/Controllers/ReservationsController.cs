using AutoMapper;
using AutoMapper.QueryableExtensions;
using Birlik.Shared.DTOs;
using Birlik.Shared.DTOs.Page;
using Birlik.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
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
    
    [HttpGet("company/{companyId}/management-page")]
    public async Task<IActionResult> GetReservationManagementPage(int companyId)
    {
        var today = DateTime.UtcNow.Date; // UTC kullanmak her zaman daha güvenlidir

        // 1. Sadece CountAsync kullanarak RAM'e veri indirmeden sayıları alıyoruz
        var requiresApprovalCount = await _context.Reservations
            .CountAsync(r => r.CompanyId == companyId && r.ReservationStatus == ReservationStatus.Pending);

        var approvedCount = await _context.Reservations
            .CountAsync(r => r.CompanyId == companyId && r.ReservationStatus == ReservationStatus.Confirmed);

        var canceledCount = await _context.Reservations
            .CountAsync(r => r.CompanyId == companyId && r.ReservationStatus == ReservationStatus.Canceled);   

        // 2. LİSTEYİ ÇEK (Grid'e basılacak seferler)
        var ReservationList = await _context.Reservations
            .Where(r => r.CompanyId == companyId)
            .ProjectTo<ReservationListDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        // 3. PAKETLE VE GÖNDER (Tek bir DTO içinde birleştiriyoruz)
        var pageData = new ReservationManagementPageDto
        {
            RequiresApprovalCount = requiresApprovalCount,
            ApprovedCount = approvedCount,
            CanceledCount = canceledCount,
            Reservations = ReservationList
        };

        return Ok(pageData); // 200 OK ile JSON olarak fırlat
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
