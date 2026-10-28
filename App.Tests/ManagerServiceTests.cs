using App.Core.DTOs.Manager;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Services;
using App.Services.Security;
using App.Services.Settings;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace App.Tests;

public class ManagerServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly PasswordHasher _hasher = new();
    private readonly ManagerService _service;

    public ManagerServiceTests()
    {
        var settings = Options.Create(new InitialManagerSettings
        {
            Name = "admin",
            Email = "Admin@Test.com",
            Password = "87654321"
        });
        _service = new ManagerService(_users, Mock.Of<IRequestRepository>(), _hasher, settings);

        _users.Seed("רחל", "rachel@test.com", _hasher.Hash("11111111"), UserRole.Responsible);
    }

    [Fact]
    public async Task CreateResponsible_NewNameAndNewEmail_Succeeds()
    {
        var result = await _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "לאה", Email = "leah@test.com" });

        Assert.Equal("לאה", result.Name);
        Assert.Equal("leah@test.com", result.Email);
        Assert.True(result.RequiresPasswordChange);
    }

    [Fact]
    public async Task CreateResponsible_ExistingNameAndNewEmail_Succeeds()
    {
        var result = await _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "רחל", Email = "rachel2@test.com" });

        Assert.Equal("רחל", result.Name);
        Assert.Equal(2, _users.Users.Count(u => u.Name == "רחל"));
    }

    [Fact]
    public async Task CreateResponsible_NewNameAndExistingEmail_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "לאה", Email = "rachel@test.com" }));
    }

    [Fact]
    public async Task CreateResponsible_ExistingNameAndExistingEmail_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "רחל", Email = "rachel@test.com" }));
    }

    [Fact]
    public async Task CreateResponsible_ExistingEmailWithDifferentCase_ThrowsConflict()
    {
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "לאה", Email = " Rachel@Test.com " }));
    }

    [Fact]
    public async Task CreateResponsible_SetsTemporaryPassword()
    {
        await _service.CreateResponsibleAsync(new CreateResponsibleDto { Name = "לאה", Email = "leah@test.com" });

        var created = _users.Users.Single(u => u.Email == "leah@test.com");
        Assert.True(_hasher.Verify("12345678", created.PasswordHash));
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task EnsureInitialManager_NoManager_CreatesManagerWithEmail()
    {
        await _service.EnsureInitialManagerAsync();

        var manager = _users.Users.Single(u => u.Role == UserRole.Manager);
        Assert.Equal("admin@test.com", manager.Email);
    }

    [Fact]
    public async Task EnsureInitialManager_ExistingManagerWithoutEmail_GetsEmailFromSettings()
    {
        var manager = _users.Seed("admin", null!, _hasher.Hash("87654321"), UserRole.Manager);

        await _service.EnsureInitialManagerAsync();

        Assert.Equal("admin@test.com", manager.Email);
        Assert.Single(_users.Users, u => u.Role == UserRole.Manager);
    }
}
