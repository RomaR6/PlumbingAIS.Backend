using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.Services;
using PlumbingAIS.Backend.Data;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StocksController : ControllerBase
    {
        private readonly IGenericRepository<Stock> _repository;
        private readonly IStockService _stockService;
        private readonly LoggerService _logger;
        private readonly AppDbContext _context;

        public StocksController(
            IGenericRepository<Stock> repository,
            IStockService stockService,
            LoggerService logger,
            AppDbContext context)
        {
            _repository = repository;
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
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _stockService.ProcessTransactionAsync(productId, locationId, quantity, type, userId, contractorId);
            if (!success) return BadRequest(new { message = "Помилка транзакції. Перевірте кількість товару на складі." });
            _logger.LogAction($"Транзакція [{type}] продукту ID:{productId} (Контрагент: {contractorId})", userId);
            return Ok(new { message = "Транзакція успішно проведена" });
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveStock(int productId, int fromLocationId, int toLocationId, decimal quantity)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _stockService.MoveStockAsync(productId, fromLocationId, toLocationId, quantity, userId);
            if (!success) return BadRequest(new { message = "Помилка переміщення. Недостатньо товару або невірні локації." });
            _logger.LogAction($"Переміщення продукту ID:{productId} з локації {fromLocationId} до {toLocationId}", userId);
            return Ok(new { message = "Переміщення успішно виконано" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id) return BadRequest();
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _repository.Update(stock);
            await _repository.SaveAsync();
            _logger.LogAction($"Ручне оновлення залишку ID:{id}", userId);
            return NoContent();
        }
    }
}