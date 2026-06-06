using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.Core.Specifications;
using Tutorial.DTOs;
using Tutorial.Entities;
using Tutorial.Enums;

namespace Tutorial.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eþleþen seferin bilgilerini getirir.
    /// </summary>
    /// <response code="200">Sefer baþarýyla bulundu. Detaylar döner.</response>
    /// <response code="404">Sefer bulunamadý.</response>
    [ProducesResponseType(typeof(DetailedTripReadDashboardDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTripById(int id)
    {
        var dto = await _context.Trips
            .Where(t => t.Id == id)
            .ProjectTo<DetailedTripReadDashboardDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return Ok(dto);
    }

    /// <summary>
    /// Yolcu görünümü: filtreye göre uygun seferleri döner.
    /// </summary>
    /// <response code="200">Ýþlem baþarýlý. Sefer listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<TripReadPassengerDto>), StatusCodes.Status200OK)]
    [HttpGet("passengerView")]
    public async Task<IActionResult> GetTrips([FromQuery] PassengerTripFilter filter)
    {
        var spec = new BookableTripsSpecification(filter);
        var query = SpecificationEvaluator<Trip>.GetQuery(_context.Trips.AsQueryable(), spec);

        var result = await query
            .ProjectTo<TripReadPassengerDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Yazýhane görünümü: filtreye göre uygun seferleri döner.
    /// </summary>
    /// <response code="200">Ýþlem baþarýlý. Sefer listesi döner.</response>
    [ProducesResponseType(typeof(IEnumerable<BasicTripReadDashboardDto>), StatusCodes.Status200OK)]
    [HttpGet("dashboardView")] // Bir yazýhane seferleri görüntülemek istediði zaman çaðrýlacak metot.  
    public async Task<IActionResult> GetTrips([FromQuery] DashboardTripFilter filter)
    {
        var spec = new FilteredCompanyTripsSpecification(filter);
        var query = SpecificationEvaluator<Trip>.GetQuery(_context.Trips.AsQueryable(), spec);

        var result = await query
            .ProjectTo<BasicTripReadDashboardDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Yeni bir sefer oluþturur.
    /// </summary>
    /// <response code="201">Sefer baþarýyla oluþturuldu. Oluþturulan nesne döner.</response>
    [ProducesResponseType(typeof(DetailedTripReadDashboardDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateTrip(TripCreateDto dto)
    {
        // 3 gün sonraki bir sefer aktif olabilir fakat satýþta olmamasý lazým. Business logic olarak ekleyebiliriz. 

        var entity = _mapper.Map<Trip>(dto);

        _context.Trips.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTripById), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Mevcut seferin alanlarýný kýsmen günceller.
    /// </summary>
    /// <response code="204">Güncelleme baþarýlý.</response>
    /// <response code="404">Sefer bulunamadý.</response>
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, TripPatchDto dto)
    {
        var entity = await _context.Trips.FindAsync(id);
        if (entity == null)
            return NotFound();

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /* Bir sefer silinemez, iptal edilebilir. Silme iþlemi yerine, TripStatus'ü iptal olarak güncellemek yeterli olur. 
     * Bu nedenle silme, silinenleri getirme ve kurtarma metotlarýný kaldýrdým. 
     */
}
