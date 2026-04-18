using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BrandsController : ControllerBase
    {
        private readonly IGenericRepository<Brand> _repository;

        public BrandsController(IGenericRepository<Brand> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _repository.GetAllAsync();
            return Ok(brands);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Brand>> GetBrand(int id)
        {
            var brand = await _repository.GetByIdAsync(id);
            if (brand == null) return NotFound();
            return Ok(brand);
        }
    }
}