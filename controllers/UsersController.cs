using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventoryApi.Controllers;

public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // =====================================
    // GET COMPANY ID FROM JWT
    // =====================================
    private int GetCompanyId()
    {
        return int.Parse(
            User.FindFirst("CompanyId")!.Value
        );
    }

    // =====================================
    // GET CURRENT USER ID
    // =====================================
    private int GetUserId()
    {
        return int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );
    }

    // =====================================
    // GET USERS
    // =====================================
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        int page = 1,
        int pageSize = 10)
    {
        var companyId = GetCompanyId();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u =>
                !u.IsDeleted &&
                u.CompanyId == companyId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // =====================================
    // GET USER
    // =====================================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var companyId = GetCompanyId();

        var user = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.Id == id &&
                !u.IsDeleted &&
                u.CompanyId == companyId)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // =====================================
    // UPDATE USER
    // =====================================
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        int id,
        [FromBody] UpdateUserDto dto)
    {
        var companyId = GetCompanyId();

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == id &&
                !u.IsDeleted &&
                u.CompanyId == companyId);

        if (user == null)
            return NotFound();

        dto.Email = dto.Email.Trim().ToLower();
        dto.FullName = dto.FullName.Trim();
        dto.Role = dto.Role.Trim();

        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.Id != id &&
                !u.IsDeleted &&
                u.CompanyId == companyId &&
                u.Email == dto.Email);

        if (emailExists)
            return BadRequest(
                "Email already exists");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Role = dto.Role;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =====================================
    // SOFT DELETE USER
    // =====================================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var companyId = GetCompanyId();
        var currentUserId = GetUserId();

        if (id == currentUserId)
        {
            return BadRequest(
                "You cannot delete your own account");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Id == id &&
                !u.IsDeleted &&
                u.CompanyId == companyId);

        if (user == null)
            return NotFound();

        user.IsDeleted = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}