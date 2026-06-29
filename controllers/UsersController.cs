using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // ============================
    // GET COMPANY ID FROM JWT
    // ============================
    private int GetCompanyId()
    {
        return int.Parse(
            User.FindFirst("CompanyId")!.Value
        );
    }

    // ============================
    // GET ALL USERS
    // api/users?page=1&pageSize=10
    // ============================
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
                u.Role
            })
            .ToListAsync();

        return Ok(users);
    }

    // ============================
    // GET SINGLE USER
    // api/users/5
    // ============================
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
                u.Role
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // ============================
    // UPDATE USER
    // api/users/5
    // ============================
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

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Role = dto.Role;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ============================
    // SOFT DELETE USER
    // api/users/5
    // ============================
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var companyId = GetCompanyId();

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