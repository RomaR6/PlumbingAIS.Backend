using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly IGenericRepository<Location> _repository;

        public LocationsController(IGenericRepository<Location> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> Get() => Ok(await _repository.GetAllAsync());

        [HttpPost]
        public async Task<ActionResult<Location>> Post(Location location)
        {
            await _repository.AddAsync(location);
            await _repository.SaveAsync();
            return Ok(location);
        }
    }
}