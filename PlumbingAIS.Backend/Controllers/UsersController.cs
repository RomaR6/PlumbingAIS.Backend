using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlumbingAIS.Backend.Interfaces;
using PlumbingAIS.Backend.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] 
public class UsersController : ControllerBase
{
    private readonly IGenericRepository<User> _userRepo;
    public UsersController(IGenericRepository<User> userRepo) => _userRepo = userRepo;

    [HttpGet] public async Task<IActionResult> GetEmployees() => Ok(await _userRepo.GetAllAsync());

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] int newRoleId)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return NotFound();
        user.RoleId = newRoleId;
        _userRepo.Update(user);
        await _userRepo.SaveAsync();
        return NoContent();
    }
}