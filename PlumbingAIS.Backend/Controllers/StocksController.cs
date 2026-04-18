using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.Services;

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

        public StocksController(IGenericRepository<Stock> repository, IStockService stockService, LoggerService logger)
        {
            _repository = repository;
            _stockService = stockService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks() => Ok(await _repository.GetAllAsync());

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction(int productId, int locationId, decimal quantity, string type)
        {
            // Беремо ID користувача з токена
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var success = await _stockService.ProcessTransactionAsync(productId, locationId, quantity, type, userId);
            if (!success) return BadRequest(new { message = "Помилка транзакції" });

            _logger.LogAction($"Транзакція [{type}] продукту ID:{productId}", userId);
            return Ok(new { message = "Успішно проведено" });
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