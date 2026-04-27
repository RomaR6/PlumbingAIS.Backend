using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILoggerService _logger;

        public ProductsController(IProductRepository repository, ILoggerService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private int GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return idClaim != null ? int.Parse(idClaim) : 1;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts([FromQuery] string? category, [FromQuery] string? material, [FromQuery] string? search)
        {
            var products = await _repository.GetAllAsync();
            var query = products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category != null && p.Category.Name.Contains(category, System.StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(material))
                query = query.Where(p => p.Material != null && p.Material.Contains(material, System.StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase) || p.SKU.Contains(search, System.StringComparison.OrdinalIgnoreCase));

            var result = query.Select(p => new ProductReadDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Price = p.Price,
                Material = p.Material,
                Diameter = p.Diameter,
                ThreadType = p.ThreadType,
                CategoryName = p.Category != null ? p.Category.Name : "Не вказано",
                BrandName = p.Brand != null ? p.Brand.Name : "Не вказано",
                UnitName = p.Unit != null ? p.Unit.Name : "Не вказано"
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductReadDto>> GetProduct(int id)
        {
            var p = await _repository.GetByIdAsync(id);
            if (p == null) return NotFound();

            return Ok(new ProductReadDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category != null ? p.Category.Name : "Не вказано"
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _repository.AddAsync(product);
            await _logger.LogActionAsync($"Створення товару: {product.Name} (SKU: {product.SKU})", GetUserId());

            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            await _repository.UpdateAsync(product);
            await _logger.LogActionAsync($"Оновлення товару: {product.Name}", GetUserId());

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return NotFound();

            string info = $"{product.Name} (SKU: {product.SKU})";
            await _repository.DeleteAsync(id);
            await _logger.LogActionAsync($"Видалення товару: {info}", GetUserId());

            return NoContent();
        }
    }
}