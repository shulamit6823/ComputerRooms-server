using App.Api.Extensions;
using App.Core.DTOs.Auth;
using App.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    /// <summary>הרשמה עצמית של מורה</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto) =>
        Ok(await _authService.RegisterAsync(dto));

    /// <summary>התחברות לפי מייל + סיסמה. אם RequiresPasswordChange=true לא מוחזר Token ויש להחליף סיסמה.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto) =>
        Ok(await _authService.LoginAsync(dto));

    /// <summary>החלפת הסיסמה הזמנית בכניסה הראשונה</summary>
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword(ChangePasswordDto dto) =>
        Ok(await _authService.ChangePasswordAsync(dto));

    /// <summary>פרטי המשתמש המחובר</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me() =>
        Ok(await _userService.GetByIdAsync(User.GetUserId()));
}
