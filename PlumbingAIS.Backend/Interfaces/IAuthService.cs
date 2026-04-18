using PlumbingAIS.Backend.DTOs;
using PlumbingAIS.Backend.Models;

namespace PlumbingAIS.Backend.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserRegisterDto dto);
        Task<string?> LoginAsync(UserLoginDto dto);
    }
}