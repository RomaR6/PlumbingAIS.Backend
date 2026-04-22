using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using PlumbingAIS.Backend.Data;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly AppDbContext _context;

        public ProductsController(IProductRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts(
            [FromQuery] string? category,
            [FromQuery] string? material,
            [FromQuery] string? search)
        {
            var products = await _repository.GetAllAsync();
            var query = products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null &&
                    p.Category.Name.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(material))
            {
                query = query.Where(p => p.Material != null &&
                    p.Material.Contains(material, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

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
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ProductReadDto>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdProduct = await _repository.AddAsync(product);

            var log = new ActionLog
            {
                Action = $"Створення товару: {product.Name} (SKU: {product.SKU})",
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                Timestamp = DateTime.Now
            };
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (oldProduct == null) return NotFound();

            await _repository.UpdateAsync(product);

            var log = new ActionLog
            {
                Action = $"Оновлення товару: {product.Name}",
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                Timestamp = DateTime.Now
            };
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null) return NotFound();

            var log = new ActionLog
            {
                Action = $"Видалення товару: {product.Name} (SKU: {product.SKU})",
                UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                Timestamp = DateTime.Now
            };

            await _repository.DeleteAsync(id);
            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}