using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly IGenericRepository<Unit> _repository;

        public UnitsController(IGenericRepository<Unit> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Unit>>> GetUnits()
        {
            var units = await _repository.GetAllAsync();
            return Ok(units);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Unit>> GetUnit(int id)
        {
            var unit = await _repository.GetByIdAsync(id);
            if (unit == null) return NotFound();
            return Ok(unit);
        }
    }
}