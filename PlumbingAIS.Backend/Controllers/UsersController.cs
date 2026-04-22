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

        public UsersController(IGenericRepository<User> userRepo, AppDbContext context)
        {
            _userRepo = userRepo;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();

            var result = users.Select(u => new
            {
                u.Id,
                u.Username,
                u.FirstName,
                u.LastName,
                u.CreatedAt,
                Role = u.Role?.Name ?? "User"
            });

            return Ok(result);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto dto)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var oldRole = user.Role?.Name ?? "User";
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);

            if (role != null)
            {
                user.RoleId = role.Id;
                _context.Users.Update(user);

                var log = new ActionLog
                {
                    Action = $"Зміна ролі користувача {user.Username}: {oldRole} -> {dto.Role}",
                    UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                    Timestamp = DateTime.Now
                };
                _context.ActionLogs.Add(log);

                await _context.SaveChangesAsync();
                return NoContent();
            }

            return BadRequest(new { message = "Такої ролі не існує" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserIdStr == id.ToString())
            {
                return BadRequest(new { message = "Ви не можете видалити власний акаунт" });
            }

            var userLogs = await _context.ActionLogs.Where(l => l.UserId == id).ToListAsync();
            foreach (var logItem in userLogs)
            {
                logItem.UserId = null;
            }

            var deleteLog = new ActionLog
            {
                Action = $"Видалення акаунту користувача: {user.Username}",
                UserId = int.Parse(currentUserIdStr!),
                Timestamp = DateTime.Now
            };

            _userRepo.Delete(user);
            _context.ActionLogs.Add(deleteLog);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class RoleUpdateDto { public string Role { get; set; } = string.Empty; }
}