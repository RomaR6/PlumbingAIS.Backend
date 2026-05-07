using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.DTOs;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _logger;

        public StocksController(IStockService stockService, IUnitOfWork unitOfWork, ILoggerService logger)
        {
            _stockService = stockService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            var stocks = await _unitOfWork.Stocks.GetAllAsync(
                s => s.Product,
                s => s.Location,
                s => s.Location.Warehouse
            );
            return Ok(stocks);
        }

        [HttpPost("transaction")]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequestDto dto)
        {
            try
            {
                int transactionId = await _stockService.ProcessGroupTransactionAsync(dto, GetUserId());
                await _logger.LogActionAsync($"Транзакція [{dto.Type}] ID:{transactionId}", GetUserId());
                return Ok(new { message = "Успішно проведено", transactionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveStock([FromBody] StockMoveRequestDto dto)
        {
            try
            {
                int transactionId = await _stockService.MoveStockAsync(
                    dto.ProductId, dto.FromLocationId, dto.ToLocationId, dto.Quantity, GetUserId(), dto.Description);

                await _logger.LogActionAsync($"Переміщення продукту ID:{dto.ProductId} TRX:{transactionId}", GetUserId());
                return Ok(new { message = "Успішно виконано", transactionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id) return BadRequest();

            _unitOfWork.Stocks.Update(stock);
            await _unitOfWork.CompleteAsync();

            await _logger.LogActionAsync($"Ручне коригування залишку ID:{id}", GetUserId());
            return NoContent();
        }
    }
}