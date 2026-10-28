using App.Core.DTOs.Auth;

namespace App.Core.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto dto);
}
