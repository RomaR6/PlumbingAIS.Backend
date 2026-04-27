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
    public class WarehousesController : ControllerBase
    {
        private readonly IGenericRepository<Warehouse> _repository;
        private readonly ILoggerService _logger;

        public WarehousesController(IGenericRepository<Warehouse> repository, ILoggerService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Warehouse>>> Get() => Ok(await _repository.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Warehouse>> Post(Warehouse warehouse)
        {
            if (warehouse == null) return BadRequest();
            await _repository.AddAsync(warehouse);
            await _repository.SaveAsync();
            await _logger.LogActionAsync($"Додано новий склад: {warehouse.Name}", GetUserId());
            return Ok(warehouse);
        }
    }
}