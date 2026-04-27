using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IGenericRepository<User> _userRepo;
        private readonly AppDbContext _context;
        private readonly ILoggerService _logger;

        public UsersController(IGenericRepository<User> userRepo, AppDbContext context, ILoggerService logger)
        {
            _userRepo = userRepo;
            _context = context;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return Ok(users.Select(u => new
            {
                u.Id,
                u.Username,
                u.FirstName,
                u.LastName,
                u.CreatedAt,
                Role = u.Role?.Name ?? "User"
            }));
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null) return BadRequest(new { message = "Такої ролі не існує" });

            var oldRole = user.Role?.Name ?? "User";
            user.RoleId = role.Id;

            _context.Users.Update(user);
            await _logger.LogActionAsync($"Зміна ролі {user.Username}: {oldRole} -> {dto.Role}", GetUserId());
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var currentId = GetUserId();
            if (currentId == id) return BadRequest(new { message = "Ви не можете видалити власний акаунт" });

            var userLogs = await _context.ActionLogs.Where(l => l.UserId == id).ToListAsync();
            foreach (var logItem in userLogs)
            {
                logItem.UserId = null;
            }
            await _context.SaveChangesAsync();

            var username = user.Username;
            await _logger.LogActionAsync($"Видалення акаунту користувача: {username}", currentId);

            _userRepo.Delete(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class RoleUpdateDto
    {
        public string Role { get; set; } = string.Empty;
    }
}