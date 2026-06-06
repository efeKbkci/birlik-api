using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

using Birlik.Shared.DTOs;

namespace Tutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        /// <summary>
        /// ID ile eşleşen şehir bilgisini getirir.
        /// </summary>
        /// <response code="200">Şehir başarıyla bulundu. Detaylar döner.</response>
        /// <response code="404">Şehir bulunamadı.</response>
        [ProducesResponseType(typeof(CityReadDto), StatusCodes.Status200OK)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCityById(int id)
        {
            var cityDto = await _context.Cities
                            .Where(d => d.Id == id)
                            .ProjectTo<CityReadDto>(_mapper.ConfigurationProvider)
                            .FirstOrDefaultAsync();

            if (cityDto == null)
                return NotFound();
            
            return Ok(cityDto);
        }

        /// <summary>
        /// Yeni bir şehir oluşturur. 
        /// </summary>
        /// <response code="201">Şehir başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
        [ProducesResponseType(typeof(CityReadDto), StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<IActionResult> CreateCity(CityCreateDto dto)
        {
            var newCity = _mapper.Map<City>(dto);

            _context.Cities.Add(newCity);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCityById), new { id = newCity.Id }, newCity);
        }

        /// <summary>
        /// ID ile eşleşen şehrin bilgilerini günceller.
        /// </summary>
        /// <response code="204">Güncelleme başarılı.</response>
        /// <response code="404">Şehir bulunamadı.</response>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCity(int id, CityPatchDto dto)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound($"City with ID = {id} doesn't exist in DB.");
            }

            _mapper.Map(dto, city);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// ID ile eşleşen şehri Soft Delete yöntemi ile siler.
        /// </summary>
        /// <response code="204">Silme başarılı.</response>
        /// <response code="404">Şehir bulunamadı.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
                return NotFound($"City with ID = {id} doesn't exist in DB.");

            city.IsDeleted = true; // Soft Delete işlemi ile veriyi görünmez yapıyoruz.

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Silinen bir şehri kurtarır ve tekrar görünür hale getirir.
        /// </summary>
        /// <response code="200">Şehir başarıyla geri yüklendi.</response>
        /// <response code="404">Şehir bulunamadı.</response>
        /// <response code="400">Şehir zaten silinmemişse döner.</response>
        [ProducesResponseType(typeof(CityDeleteIncludedDto), StatusCodes.Status200OK)]
        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreCity(int id)
        {
            var city = await _context.Cities
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
                return NotFound();

            if (!city.IsDeleted)
                return BadRequest();

            city.IsDeleted = false;
            await _context.SaveChangesAsync();

            var cityDto = _mapper.Map<CityDeleteIncludedDto>(city);

            return Ok(cityDto);
        }
    }
}
