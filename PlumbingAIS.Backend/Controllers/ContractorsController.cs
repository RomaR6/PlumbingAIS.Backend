using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractorsController : ControllerBase
    {
        private readonly IGenericRepository<Contractor> _repository;

        public ContractorsController(IGenericRepository<Contractor> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contractor>>> GetAll()
        {
            var contractors = await _repository.GetAllAsync();
            return Ok(contractors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contractor>> GetById(int id)
        {
            var contractor = await _repository.GetByIdAsync(id);
            if (contractor == null) return NotFound();
            return Ok(contractor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Contractor>> Create(Contractor contractor)
        {
            if (contractor == null) return BadRequest();

            await _repository.AddAsync(contractor);
            await _repository.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = contractor.Id }, contractor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var contractor = await _repository.GetByIdAsync(id);
            if (contractor == null) return NotFound();

            _repository.Delete(contractor);
            await _repository.SaveAsync();
            return NoContent();
        }
    }
}