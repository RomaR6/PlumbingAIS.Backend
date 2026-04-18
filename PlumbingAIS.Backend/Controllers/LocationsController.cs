using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationsController : ControllerBase
    {
        private readonly IGenericRepository<Location> _repository;

        public LocationsController(IGenericRepository<Location> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Location>>> Get() => Ok(await _repository.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult<Location>> Post(Location location)
        {
            await _repository.AddAsync(location);
            await _repository.SaveAsync();
            return Ok(location);
        }
    }
}