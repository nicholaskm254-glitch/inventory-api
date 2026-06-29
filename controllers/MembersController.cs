using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InventoryApi.Controllers
{   [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private int GetCompanyId()
{
    return int.Parse(
        User.FindFirst("CompanyId")!.Value
    );
}

        public MembersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/members
         [HttpGet]
public async Task<IActionResult> GetMembers()
{
    var companyId = GetCompanyId();

    var members = await _context.Members
        .AsNoTracking()
        .Where(m => m.CompanyId == companyId)
        .ToListAsync();

    return Ok(members);
}

        // POST: api/members
        [HttpPost]
        public async Task<IActionResult> AddMember(Member member)
        {
            // 🔹 Validation
            if (member == null)
                return BadRequest("Invalid member data");

            if (string.IsNullOrWhiteSpace(member.FullName) ||
                string.IsNullOrWhiteSpace(member.Role))
            {
                return BadRequest("FullName and Role are required");
            }

            // 🔹 Normalize data
            member.FullName = member.FullName.Trim().ToUpper();
            member.Role = member.Role.Trim().ToUpper();

            // 🔹 Check duplicates
            var exists = await _context.Members.AnyAsync(m =>
                m.FullName == member.FullName &&
                m.Role == member.Role
            );

            if (exists)
                return Conflict("Member already exists");

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMembers), member);
        }

        // DELETE: api/members/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
                return NotFound("Member not found");

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}