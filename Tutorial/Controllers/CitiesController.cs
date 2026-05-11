using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tutorial.Context;
using Tutorial.DTOs;
using Tutorial.Entities;


namespace Tutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController(AppDbContext context, IMapper mapper) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;   

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

        [HttpPost]
        public async Task<IActionResult> CreateCity(CityCreateDto dto)
        {
            var newCity = _mapper.Map<City>(dto);

            _context.Cities.Add(newCity);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCityById), new { id = newCity.Id }, newCity);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateCity(int id, CityPatchDto dto)
        {
            var city = await _context.Cities.FindAsync(id);

            if (city == null)
            {
                return NotFound($"City with ID = {id} doesn't exist in DB.");
            }

            city.Name = dto.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

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

        [HttpPut("{id}/restore")]
        public async Task<IActionResult> RestoreCity(int id)
        {
            var city = await _context.Cities
                                .IgnoreQueryFilters()
                                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
                return NotFound($"{id} numaralı şehir veri tabanında yok.");

            if (!city.IsDeleted)
                return BadRequest("Bu şehir zaten silinmemiş.");

            city.IsDeleted = false;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Şehir başarıyla kurtarıldı." });
        }
    }
}
