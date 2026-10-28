using App.Core.DTOs.Auth;

namespace App.Core.Interfaces.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id);
}
