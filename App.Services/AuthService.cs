using App.Core.DTOs.Auth;
using App.Core.Entities;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Core.Interfaces.Services;
using App.Services.Mapping;

namespace App.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    /// <summary>הרשמה עצמית - רק למורה</summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var name = dto.Name.Trim();

        var email = EmailNormalizer.Normalize(dto.Email);
        if (await _userRepository.EmailExistsAsync(email))
            throw new ConflictException("כתובת המייל כבר רשומה במערכת.");

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            RequiresPasswordChange = false,
            IsActive = true,
            Role = UserRole.Teacher,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await GetVerifiedUserAsync(dto.Email, dto.Password);

        // אחראית עם סיסמה זמנית לא מקבלת Token עד שתחליף סיסמה
        if (user.RequiresPasswordChange)
        {
            return new AuthResponseDto
            {
                RequiresPasswordChange = true,
                User = user.ToDto()
            };
        }

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await GetVerifiedUserAsync(dto.Email, dto.CurrentPassword);

        if (!user.RequiresPasswordChange)
            throw new BusinessRuleException("המשתמש אינו נדרש להחליף סיסמה.");

        if (dto.NewPassword == dto.CurrentPassword)
            throw new BusinessRuleException("הסיסמה החדשה חייבת להיות שונה מהסיסמה הזמנית.");

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.RequiresPasswordChange = false;
        await _userRepository.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    private async Task<User> GetVerifiedUserAsync(string email, string password)
    {
        var user = await _userRepository.GetActiveByEmailAsync(EmailNormalizer.Normalize(email));

        if (user == null || !user.IsActive || !_passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("המייל או הסיסמה אינם נכונים");

        return user;
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);
        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            RequiresPasswordChange = false,
            User = user.ToDto()
        };
    }
}
