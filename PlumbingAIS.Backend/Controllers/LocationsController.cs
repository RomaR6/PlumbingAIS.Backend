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
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly IGenericRepository<Location> _repository;
        private readonly AppDbContext _context;
        private readonly ILoggerService _logger;

        public LocationsController(IGenericRepository<Location> repository, AppDbContext context, ILoggerService logger)
        {
            _repository = repository;
            _context = context;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Location>>> Get()
        {
            var locations = await _context.Locations.Include(l => l.Warehouse).ToListAsync();
            return Ok(locations);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Location>> Post(Location location)
        {
            await _repository.AddAsync(location);
            await _repository.SaveAsync();
            await _logger.LogActionAsync($"Створено нову локацію: Ряд {location.RowCode}, Стелаж {location.RackCode}", GetUserId());
            return Ok(location);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            if (await _context.Stocks.AnyAsync(s => s.LocationId == id && s.Quantity > 0))
                return BadRequest(new { message = "Неможливо видалити локацію, оскільки на ній є товари." });

            var info = $"Ряд {location.RowCode}, Стелаж {location.RackCode}";
            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            await _logger.LogActionAsync($"Видалено локацію: {info}", GetUserId());
            return NoContent();
        }
    }
}