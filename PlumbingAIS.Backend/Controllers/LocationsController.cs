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
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly IGenericRepository<Location> _repository;
        private readonly AppDbContext _context;

        public LocationsController(IGenericRepository<Location> repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Location>>> Get()
        {
            var locations = await _context.Locations
                .Include(l => l.Warehouse)
                .ToListAsync();
            return Ok(locations);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Location>> Post(Location location)
        {
            await _repository.AddAsync(location);
            await _repository.SaveAsync();
            return Ok(location);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            var hasActiveStock = await _context.Stocks.AnyAsync(s => s.LocationId == id && s.Quantity > 0);
            if (hasActiveStock)
            {
                return BadRequest(new { message = "Неможливо видалити локацію, оскільки на ній зберігаються товари. Спочатку перемістіть їх." });
            }

            var emptyStocks = await _context.Stocks.Where(s => s.LocationId == id).ToListAsync();
            if (emptyStocks.Any())
            {
                _context.Stocks.RemoveRange(emptyStocks);
            }

            try
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Локація використовується в історії операцій і не може бути видалена." });
            }
        }
    }
}