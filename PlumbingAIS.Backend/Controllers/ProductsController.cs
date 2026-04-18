using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
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
                CategoryName = p.Category != null ? p.Category.Name : "Не вказано",
                BrandName = p.Brand != null ? p.Brand.Name : "Не вказано",
                UnitName = p.Unit != null ? p.Unit.Name : "Не вказано"
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> CreateProduct(ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                SKU = dto.SKU,
                Name = dto.Name,
                Price = dto.Price,
                MinThreshold = dto.MinThreshold,
                Material = dto.Material,
                Diameter = dto.Diameter,
                ThreadType = dto.ThreadType,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                UnitId = dto.UnitId
            };

            var createdProduct = await _repository.AddAsync(product);
            var readDto = new ProductReadDto { Id = createdProduct.Id, Name = createdProduct.Name };

            return CreatedAtAction(nameof(GetProducts), new { id = readDto.Id }, readDto);
        }
    }
}