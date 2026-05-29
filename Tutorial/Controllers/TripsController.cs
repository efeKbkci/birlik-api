using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTripById(int id) 
    {
        var dto = await _context.Trips
            .Where(t => t.Id == id)
            .ProjectTo<DetailedTripReadDashboardDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return Ok(dto);
    }

    [HttpGet("passengerView")] // Bir yolcu seferleri görüntülemek istediði zaman çaðrýlacak metot.  
    public async Task<IActionResult> GetTrips([FromQuery] PassengerTripFilter filter)
    {
        // 1. ZAMANI YAKALA: Botun mesajý attýðý o kritik an (Örn: 23.05.2026 14:46:12)
        DateTime messageTime = DateTime.UtcNow;

        // 2. BUGÜNÜN GECE YARISI: Saati sýfýrla (Örn: 23.05.2026 00:00:00)
        DateTime todayMidnight = messageTime.Date;

        DateTime startTime;
        DateTime endTime;

        // Kullanýcý belirtmediyse (null ise) veya kasten Today seçtiyse
        if (filter.DaySelection == null || filter.DaySelection == TripDaySelection.Today)
        {
            // ----------------------------------------------------------------
            // SENARYO 1: BUGÜN (23.05.2026 14:46 - 23.05.2026 23:59)
            // ----------------------------------------------------------------
            startTime = messageTime; // Baþlangýç: 23.05.2026 14:46
            endTime = todayMidnight.AddDays(1); // Bitiþ: 24.05.2026 00:00 (Yarýnýn baþlangýcý)
        }
        else
        {
            // ----------------------------------------------------------------
            // SENARYO 2: YARIN (24.05.2026 00:00 - 24.05.2026 23:59)
            // ----------------------------------------------------------------
            startTime = todayMidnight.AddDays(1); // Baþlangýç: 24.05.2026 00:00
            endTime = startTime.AddDays(1); // Bitiþ: 25.05.2026 00:00
        }

        // 3. VERÝ TABANI SORGUSU (SQL SEVÝYESÝ)
        var query = _context.Trips.Where(t =>
            t.CompanyId == filter.CompanyId.Value &&
            t.RouteId == filter.RouteId.Value &&
            t.DepartureTime >= startTime &&  // Baþlangýç saatine EÞÝT veya BÜYÜK
            t.DepartureTime < endTime &&     // Bitiþ saatinden KESÝNLÝKLE KÜÇÜK
            t.TripStatus == TripStatus.OnSale &&
            t.Capacity > t.PassengerNumbers
        );

        var result = await query
            .ProjectTo<TripReadPassengerDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("dashboardView")] // Bir yazýhane seferleri görüntülemek istediði zaman çaðrýlacak metot.  
    public async Task<IActionResult> GetTrips([FromQuery] DashboardTripFilter filter)
    {
        // 1. Temel kural: Yazýhane kesinlikle kendi ID'sini yollamalý.
        var query = _context.Trips.Where(t => t.CompanyId == filter.CompanyId);

        // 2. Esnek Filtreler (Hangisi gönderildiyse o LINQ sorgusuna eklenir)
        if (filter.RouteId.HasValue)
            query = query.Where(t => t.RouteId == filter.RouteId);

        if (filter.VehicleId.HasValue)
            query = query.Where(t => t.VehicleId == filter.VehicleId);

        if (filter.DriverId.HasValue)
            query = query.Where(t => t.DriverId == filter.DriverId);

        if (filter.Status.HasValue)
            query = query.Where(t => t.TripStatus == filter.Status);

        // Tarih Aralýðý Filtresi
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.DepartureTime >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(t => t.DepartureTime <= filter.EndDate.Value);

        var result = await query
            .ProjectTo<BasicTripReadDashboardDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrip(TripCreateDto dto)
    {
        // 3 gün sonraki bir sefer aktif olabilir fakat satýþta olmamasý lazým. Business logic olarak ekleyebiliriz. 

        var entity = _mapper.Map<Trip>(dto);

        _context.Trips.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTripById), new { id = entity.Id }, entity);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTrip(int id, TripPatchDto dto)
    {
        var entity = await _context.Trips.FindAsync(id);
        if (entity == null)
            return NotFound($"Trip with ID = {id} doesn't exist in DB.");

        _mapper.Map(dto, entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /* Bir sefer silinemez, iptal edilebilir. Silme iþlemi yerine, TripStatus'ü iptal olarak güncellemek yeterli olur. 
     * Bu nedenle silme, silinenleri getirme ve kurtarma metotlarýný kaldýrdým. 
     */
}
