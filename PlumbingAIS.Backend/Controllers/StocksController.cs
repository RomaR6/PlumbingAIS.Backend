using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Data;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.DTOs;
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
                    .ThenInclude(l => l != null ? l.Warehouse : null)
                .ToListAsync();

            return Ok(stocks);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            int transactionId = await _stockService.ProcessGroupTransactionAsync(dto, userId);

            if (transactionId == 0)
                return BadRequest(new { message = "Помилка транзакції. Перевірте наявність товару на складі." });

            var emptyStocks = _context.Stocks.Where(s => s.Quantity <= 0);
            _context.Stocks.RemoveRange(emptyStocks);
            await _context.SaveChangesAsync();

            await _logger.LogActionAsync($"Транзакція [{dto.Type}] ID:{transactionId}", userId);

            return Ok(new { message = "Транзакція успішно проведена", transactionId = transactionId });
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveStock([FromBody] StockMoveRequestDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            int transactionId = await _stockService.MoveStockAsync(
                dto.ProductId,
                dto.FromLocationId,
                dto.ToLocationId,
                dto.Quantity,
                userId,
                dto.Description);

            if (transactionId == 0)
                return BadRequest(new { message = "Помилка переміщення. Перевірте залишки." });

            var emptyStocks = _context.Stocks.Where(s => s.Quantity <= 0);
            _context.Stocks.RemoveRange(emptyStocks);
            await _context.SaveChangesAsync();

            await _logger.LogActionAsync($"Переміщення продукту ID:{dto.ProductId} ID:{transactionId}", userId);

            return Ok(new { message = "Переміщення успішно виконано", transactionId = transactionId });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id) return BadRequest();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = userIdClaim != null ? int.Parse(userIdClaim) : 1;

            if (stock.Quantity <= 0)
            {
                var existingStock = await _context.Stocks.FindAsync(id);
                if (existingStock != null) _context.Stocks.Remove(existingStock);
            }
            else
            {
                _context.Entry(stock).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            await _logger.LogActionAsync($"Оновлення залишку ID:{id}", userId);
            return NoContent();
        }
    }
}