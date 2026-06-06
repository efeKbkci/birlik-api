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
public class AdminsController(AppDbContext context, IMapper mapper) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// ID ile eşleşen admin bilgisini getirir.
    /// </summary>
    /// <response code="200">İşlem başarılı. Admin bilgileri döner.</response>
    /// <response code="404">Admin bulunamadı veya silinmiş.</response>
    [ProducesResponseType(typeof(AdminReadDto), StatusCodes.Status200OK)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAdminById(int id)
    {
        var adminDto = await _context.Admins
            .Where(a => a.Id == id)
            .ProjectTo<AdminReadDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (adminDto == null)
            return NotFound($"Admin with ID = {id} doesn't exist or is deleted.");

        return Ok(adminDto);
    }

    /// <summary>
    /// Yeni bir admin oluşturur.
    /// </summary>
    /// <response code="201">Admin başarıyla oluşturuldu. Oluşturulan nesne döner.</response>
    /// <response code="409">Telefon veya e-posta çakışması durumunda döner.</response>
    [ProducesResponseType(typeof(AdminReadDto), StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<IActionResult> CreateAdmin(AdminCreateDto dto)
    {
        // 1. İş Kuralı: Telefon numarası veya e-posta adresi benzersiz olmalı.
        bool isUserExist = await _context.Admins.AnyAsync(a =>
            a.PhoneNumber == dto.PhoneNumber ||
            a.Email == dto.Email);

        if (isUserExist)
            return Conflict("Bu telefon numarası veya e-posta adresi zaten sistemde kayıtlı.");

        var newAdmin = _mapper.Map<Admin>(dto);

        // 3. Şifreyi BCrypt ile Hashle.
        newAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        _context.Admins.Add(newAdmin);
        await _context.SaveChangesAsync();

        var resultDto = _mapper.Map<AdminReadDto>(newAdmin);

        return CreatedAtAction(nameof(GetAdminById), new { id = newAdmin.Id }, resultDto);
    }

    /// <summary>
    /// Admin için giriş doğrulaması (preview).
    /// </summary>
    /// <response code="200">Giriş başarılı.</response>
    /// <response code="401">Geçersiz kimlik bilgileri.</response>
    [HttpPost("login-preview")]
    public async Task<IActionResult> LoginPreview([FromBody] AdminLoginDto request)
    {
        // E-posta veya Telefon numarası ile eşleşen admini bul
        var admin = await _context.Admins.FirstOrDefaultAsync(a =>
            a.Email == request.EmailOrPhone ||
            a.PhoneNumber == request.EmailOrPhone);

        if (admin == null)
            return Unauthorized("Geçersiz e-posta/telefon numarası veya şifre.");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized("Geçersiz e-posta/telefon numarası veya şifre.");

        // 3. Başarılı giriş. LoginAt tarihini güncelle
        admin.LoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    /// Admini soft-delete ile görünmez yapar.
    /// </summary>
    /// <response code="204">Silme başarılı.</response>
    /// <response code="404">Admin bulunamadı.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdmin(int id)
    {
        var admin = await _context.Admins.FindAsync(id);

        if (admin == null)
            return NotFound($"Admin with ID = {id} doesn't exist.");

        admin.IsDeleted = true; // Soft Delete
        await _context.SaveChangesAsync();

        return NoContent();
    }
}