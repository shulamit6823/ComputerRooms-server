using App.Core.DTOs.Auth;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Services;
using App.Services;
using App.Services.Security;
using Moq;
using Xunit;

namespace App.Tests;

public class AuthServiceTests
{
    private const string TemporaryPassword = "12345678";

    private readonly FakeUserRepository _users = new();
    private readonly PasswordHasher _hasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService
            .Setup(t => t.CreateToken(It.IsAny<App.Core.Entities.User>()))
            .Returns((App.Core.Entities.User u) => ($"token-{u.Id}", DateTime.UtcNow.AddHours(1)));

        _service = new AuthService(_users, _hasher, tokenService.Object);
    }

    [Theory]
    [InlineData(UserRole.Manager)]
    [InlineData(UserRole.Teacher)]
    [InlineData(UserRole.Responsible)]
    public async Task Login_WithEmailAndPassword_ReturnsToken(UserRole role)
    {
        var user = _users.Seed("משתמש", "user@test.com", _hasher.Hash("password1"), role);

        var result = await _service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "password1" });

        Assert.Equal($"token-{user.Id}", result.Token);
        Assert.False(result.RequiresPasswordChange);
        Assert.Equal(role, result.User.Role);
    }

    [Fact]
    public async Task Login_EmailIsCaseInsensitive()
    {
        _users.Seed("משתמש", "user@test.com", _hasher.Hash("password1"), UserRole.Teacher);

        var result = await _service.LoginAsync(new LoginDto { Email = " User@Test.COM ", Password = "password1" });

        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_TwoUsersWithSameName_EachLogsInWithOwnEmail()
    {
        var first = _users.Seed("רחל", "rachel1@test.com", _hasher.Hash("password1"), UserRole.Responsible);
        var second = _users.Seed("רחל", "rachel2@test.com", _hasher.Hash("password2"), UserRole.Responsible);

        var r1 = await _service.LoginAsync(new LoginDto { Email = "rachel1@test.com", Password = "password1" });
        var r2 = await _service.LoginAsync(new LoginDto { Email = "rachel2@test.com", Password = "password2" });

        Assert.Equal(first.Id, r1.User.Id);
        Assert.Equal(second.Id, r2.User.Id);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        _users.Seed("משתמש", "user@test.com", _hasher.Hash("password1"), UserRole.Teacher);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "wrong-password" }));
    }

    [Fact]
    public async Task Login_InactiveUser_ThrowsUnauthorized()
    {
        _users.Seed("משתמש", "user@test.com", _hasher.Hash("password1"), UserRole.Responsible, isActive: false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginDto { Email = "user@test.com", Password = "password1" }));
    }

    [Fact]
    public async Task Login_ResponsibleWithTemporaryPassword_RequiresPasswordChangeWithoutToken()
    {
        _users.Seed("רחל", "rachel@test.com", _hasher.Hash(TemporaryPassword), UserRole.Responsible, requiresPasswordChange: true);

        var result = await _service.LoginAsync(new LoginDto { Email = "rachel@test.com", Password = TemporaryPassword });

        Assert.True(result.RequiresPasswordChange);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task ChangePassword_WithEmail_ReturnsTokenAndTemporaryPasswordStopsWorking()
    {
        var user = _users.Seed("רחל", "rachel@test.com", _hasher.Hash(TemporaryPassword), UserRole.Responsible, requiresPasswordChange: true);

        var result = await _service.ChangePasswordAsync(new ChangePasswordDto
        {
            Email = "rachel@test.com",
            CurrentPassword = TemporaryPassword,
            NewPassword = "78945612"
        });

        Assert.Equal($"token-{user.Id}", result.Token);
        Assert.False(result.RequiresPasswordChange);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginDto { Email = "rachel@test.com", Password = TemporaryPassword }));

        var login = await _service.LoginAsync(new LoginDto { Email = "rachel@test.com", Password = "78945612" });
        Assert.NotNull(login.Token);
    }

    [Fact]
    public async Task ChangePassword_SameNameDifferentEmail_ChangesOnlyThatUser()
    {
        var other = _users.Seed("רחל", "rachel1@test.com", _hasher.Hash(TemporaryPassword), UserRole.Responsible, requiresPasswordChange: true);
        var target = _users.Seed("רחל", "rachel2@test.com", _hasher.Hash(TemporaryPassword), UserRole.Responsible, requiresPasswordChange: true);

        await _service.ChangePasswordAsync(new ChangePasswordDto
        {
            Email = "rachel2@test.com",
            CurrentPassword = TemporaryPassword,
            NewPassword = "78945612"
        });

        Assert.False(target.RequiresPasswordChange);
        Assert.True(other.RequiresPasswordChange);
    }

    [Fact]
    public async Task Register_ExistingNameAndNewEmail_Succeeds()
    {
        _users.Seed("רחל", "rachel@test.com", _hasher.Hash("password1"), UserRole.Teacher);

        var result = await _service.RegisterAsync(new RegisterDto { Name = "רחל", Email = "rachel2@test.com", Password = "password2" });

        Assert.NotNull(result.Token);
        Assert.Equal(2, _users.Users.Count(u => u.Name == "רחל"));
    }

    [Fact]
    public async Task Register_ExistingEmail_ThrowsConflict()
    {
        _users.Seed("רחל", "rachel@test.com", _hasher.Hash("password1"), UserRole.Teacher);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(new RegisterDto { Name = "לאה", Email = "rachel@test.com", Password = "password2" }));
    }
}
