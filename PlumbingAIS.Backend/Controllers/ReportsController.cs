using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] 
    public class ReportsController : ControllerBase
    {
        private readonly IStockService _stockService;

        public ReportsController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet("total-value")]
        public async Task<IActionResult> GetTotalValue()
        {
            var value = await _stockService.GetTotalStockValueAsync();
            return Ok(new { totalValue = value, currency = "UAH" });
        }

        [HttpGet("inventory-check")]
        public async Task<IActionResult> GetInventoryReport()
        {
            var critical = await _stockService.GetCriticalStocksAsync();
            return Ok(new
            {
                reportDate = DateTime.Now,
                criticalItemsCount = critical.Count(),
                items = critical
            });
        }
    }
}