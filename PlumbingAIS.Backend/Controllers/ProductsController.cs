using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts()
        {
            var products = await _repository.GetAllAsync();

            var result = products.Select(p => new ProductReadDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                Price = p.Price,
                Material = p.Material,
                Diameter = p.Diameter,
                CategoryName = p.Category?.Name ?? "Не вказано",
                BrandName = p.Brand?.Name ?? "Не вказано",
                UnitName = p.Unit?.Name ?? "Не вказано"
            });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ProductReadDto>> CreateProduct(ProductCreateDto dto)
        {
            if (dto == null) return BadRequest();

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

            var readDto = new ProductReadDto
            {
                Id = createdProduct.Id,
                SKU = createdProduct.SKU,
                Name = createdProduct.Name,
                Price = createdProduct.Price,
                Material = createdProduct.Material,
                Diameter = createdProduct.Diameter,
                CategoryName = "Створено"
            };

            return CreatedAtAction(nameof(GetProducts), new { id = readDto.Id }, readDto);
        }
    }
}