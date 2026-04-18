using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class WarehousesController : ControllerBase
    {
        private readonly IGenericRepository<Warehouse> _repository;

        public WarehousesController(IGenericRepository<Warehouse> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous] 
        public async Task<ActionResult<IEnumerable<Warehouse>>> Get()
            => Ok(await _repository.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult<Warehouse>> Post(Warehouse warehouse)
        {
            if (warehouse == null) return BadRequest();

            await _repository.AddAsync(warehouse);
            await _repository.SaveAsync();

            return Ok(warehouse);
        }
    }
}