using Microsoft.AspNetCore.Mvc;
using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/members
        [HttpGet]
        public async Task<IActionResult> GetMembers()
        {
            var members = await _context.Members.ToListAsync();
            return Ok(members);
        }

        // POST: api/members
        [HttpPost]
        public async Task<IActionResult> AddMember(Member member)
        {
            // normalize (important since you're using uppercase)
            member.FullName = member.FullName?.ToUpper() ?? string.Empty;
            member.Role = member.Role?.ToUpper() ?? string.Empty;

            var exists = await _context.Members.AnyAsync(m => m.FullName == member.FullName && m.Role == member.Role);

            if (exists)
            {
                return BadRequest("Member already exists");
            }

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
    var member = await _context.Members.FindAsync(id);

    if (member == null)
        return NotFound("Member not found");

    _context.Members.Remove(member);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Member deleted successfully" });
}
    }
}