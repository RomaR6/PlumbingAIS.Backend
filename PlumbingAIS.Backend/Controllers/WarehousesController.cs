using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehousesController : ControllerBase
    {
        private readonly IGenericRepository<Warehouse> _repository;

        public WarehousesController(IGenericRepository<Warehouse> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> Get() => Ok(await _repository.GetAllAsync());

        [HttpPost]
        public async Task<ActionResult<Warehouse>> Post(Warehouse warehouse)
        {
            await _repository.AddAsync(warehouse);
            await _repository.SaveAsync();
            return Ok(warehouse);
        }
    }
}