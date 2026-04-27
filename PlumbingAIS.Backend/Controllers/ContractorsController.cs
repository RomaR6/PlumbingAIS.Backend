using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;
using System.Security.Claims;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractorsController : ControllerBase
    {
        private readonly IGenericRepository<Contractor> _repository;
        private readonly ILoggerService _logger;

        public ContractorsController(IGenericRepository<Contractor> repository, ILoggerService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contractor>>> GetAll() => Ok(await _repository.GetAllAsync());

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
            await _logger.LogActionAsync($"Додано нового контрагента: {contractor.Name}", GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = contractor.Id }, contractor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var contractor = await _repository.GetByIdAsync(id);
            if (contractor == null) return NotFound();
            var name = contractor.Name;
            _repository.Delete(contractor);
            await _repository.SaveAsync();
            await _logger.LogActionAsync($"Видалено контрагента: {name}", GetUserId());
            return NoContent();
        }
    }
}