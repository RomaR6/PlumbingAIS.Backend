using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Models;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, ILoggerService logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductReadDto>>> GetProducts([FromQuery] string? category, [FromQuery] string? material, [FromQuery] string? search)
        {
            
            var products = await _unitOfWork.Products.GetAllAsync(
                p => p.Category,
                p => p.Brand,
                p => p.Unit
            );

            var query = products.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(material))
            {
                query = query.Where(p => p.Material != null && p.Material.Contains(material, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(_mapper.Map<IEnumerable<ProductReadDto>>(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductReadDto>> GetProduct(int id)
        {
            
            var products = await _unitOfWork.Products.GetAllAsync(p => p.Category, p => p.Brand, p => p.Unit);
            var product = products.FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductReadDto>(product));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ProductReadDto>> CreateProduct(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            
            var createdProduct = (await _unitOfWork.Products.GetAllAsync(p => p.Category, p => p.Brand, p => p.Unit))
                                 .FirstOrDefault(p => p.Id == product.Id);

            await _logger.LogActionAsync($"Створення товару: {product.Name}", GetUserId());

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, _mapper.Map<ProductReadDto>(createdProduct));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateProduct(int id, ProductCreateDto dto)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
            if (existingProduct == null) return NotFound();

            _mapper.Map(dto, existingProduct);
            _unitOfWork.Products.Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            await _logger.LogActionAsync($"Оновлення товару ID:{id}", GetUserId());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            await _logger.LogActionAsync($"Видалення товару ID:{id}", GetUserId());
            return NoContent();
        }
    }
}