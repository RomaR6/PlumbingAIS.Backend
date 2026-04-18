using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.Services;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IGenericRepository<Stock> _repository;
        private readonly IStockService _stockService;
        private readonly LoggerService _logger;

        public StocksController(
            IGenericRepository<Stock> repository,
            IStockService stockService,
            LoggerService logger)
        {
            _repository = repository;
            _stockService = stockService;
            _logger = logger;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            var stocks = await _repository.GetAllAsync();
            return Ok(stocks);
        }

        
        [HttpGet("critical")]
        public async Task<IActionResult> GetCriticalStocks()
        {
            var critical = await _stockService.GetCriticalStocksAsync();
            return Ok(critical);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(int id)
        {
            var stock = await _repository.GetByIdAsync(id);
            if (stock == null) return NotFound();
            return Ok(stock);
        }

        
        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction(int productId, int locationId, decimal quantity, string type, int userId)
        {
            
            var success = await _stockService.ProcessTransactionAsync(productId, locationId, quantity, type, userId);

            if (!success)
            {
                return BadRequest(new { message = "Транзакція відхилена: недостатньо товару на складі або невірна локація." });
            }

            
            _logger.LogAction($"Складна транзакція [{type}] для продукту ID:{productId}", userId);

            return Ok(new { message = "Транзакцію успішно проведено" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id) return BadRequest();

            _repository.Update(stock);
            await _repository.SaveAsync();

            _logger.LogAction($"Ручне оновлення залишку ID:{id}", 1); 

            return NoContent();
        }
    }
}