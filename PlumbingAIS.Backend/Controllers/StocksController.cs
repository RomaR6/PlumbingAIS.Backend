using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IGenericRepository<Stock> _repository;

        public StocksController(IGenericRepository<Stock> repository)
        {
            _repository = repository;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Stock>>> GetStocks()
        {
            var stocks = await _repository.GetAllAsync();
            return Ok(stocks);
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Stock>> GetStock(int id)
        {
            var stock = await _repository.GetByIdAsync(id);

            if (stock == null)
            {
                return NotFound();
            }

            return Ok(stock);
        }

        
        [HttpPost]
        public async Task<ActionResult<Stock>> CreateStock(Stock stock)
        {
            await _repository.AddAsync(stock);
            await _repository.SaveAsync();

            return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, stock);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStock(int id, Stock stock)
        {
            if (id != stock.Id)
            {
                return BadRequest();
            }

            _repository.Update(stock);
            await _repository.SaveAsync();

            return NoContent();
        }
    }
}