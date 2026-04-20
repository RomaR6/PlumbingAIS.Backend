using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

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
                Role = u.Role?.Name ?? "User" 
            });

            return Ok(result);
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto dto)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role);

            if (role != null)
            {
                user.RoleId = role.Id; 
                _userRepo.Update(user);
                await _userRepo.SaveAsync();
                return NoContent();
            }

            return BadRequest("Такої ролі не існує");
        }
    }

    public class RoleUpdateDto { public string Role { get; set; } = string.Empty; }
}