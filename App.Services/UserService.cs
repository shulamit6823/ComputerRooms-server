using App.Core.DTOs.Auth;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Core.Interfaces.Services;
using App.Services.Mapping;

namespace App.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || !user.IsActive)
            throw new NotFoundException("המשתמש לא נמצא.");

        return user.ToDto();
    }
}
