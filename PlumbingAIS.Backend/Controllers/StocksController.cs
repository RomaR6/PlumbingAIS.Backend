using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StocksController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly ILoggerService _logger;
        private readonly AppDbContext _context;

        public StocksController(IStockService stockService, ILoggerService logger, AppDbContext context)
        {
            _stockService = stockService;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            var stocks = await _context.Stocks
                .Include(s => s.Product)
                .Include(s => s.Location)
                    .ThenInclude(l => l.Warehouse)
                .ToListAsync();

            return Ok(stocks);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction(int productId, int locationId, decimal quantity, string type, int? contractorId = null)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim) : 1;

            var success = await _stockService.ProcessTransactionAsync(productId, locationId, quantity, type, userId, contractorId);

            if (!success)
                return BadRequest(new { message = "Помилка транзакції" });

            await _logger.LogActionAsync($"Транзакція [{type}] продукту ID:{productId}", userId);

            return Ok(new { message = "Транзакція успішно проведена" });
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveStock(int productId, int fromLocationId, int toLocationId, decimal quantity)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim) : 1;

            var success = await _stockService.MoveStockAsync(productId, fromLocationId, toLocationId, quantity, userId);

            if (!success)
                return BadRequest(new { message = "Помилка переміщення" });

            await _logger.LogActionAsync($"Переміщення продукту ID:{productId} з {fromLocationId} до {toLocationId}", userId);

            return Ok(new { message = "Переміщення успішно виконано" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id) return BadRequest();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim) : 1;

            _context.Entry(stock).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            await _logger.LogActionAsync($"Ручне оновлення залишку ID:{id}", userId);
            return NoContent();
        }
    }
}