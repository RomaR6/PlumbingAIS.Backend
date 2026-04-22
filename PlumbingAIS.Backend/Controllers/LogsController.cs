using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Data;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class LogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var logs = await _context.ActionLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.Timestamp)
                .Select(l => new
                {
                    l.Id,
                    l.Action,
                    l.Timestamp,
                    Username = l.User != null ? l.User.Username : "Система"
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}