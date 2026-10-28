using App.Core.Entities;
using App.Core.Enums;
using App.Core.Interfaces.Repositories;

namespace App.Tests;

/// <summary>מאגר משתמשים בזיכרון - מתנהג כמו UserRepository בלי מסד נתונים</summary>
public class FakeUserRepository : IUserRepository
{
    private int _nextId = 1;

    public List<User> Users { get; } = new();

    public Task<User?> GetByIdAsync(int id) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetActiveByEmailAsync(string email) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Email == email && u.IsActive));

    public Task<User?> GetByRoleAsync(UserRole role) =>
        Task.FromResult(Users.OrderBy(u => u.Id).FirstOrDefault(u => u.Role == role));

    public Task<bool> EmailExistsAsync(string email) =>
        Task.FromResult(Users.Any(u => u.Email == email));

    public Task<bool> ActiveEmailExistsAsync(string email) =>
        Task.FromResult(Users.Any(u => u.Email == email && u.IsActive));

    public Task<List<User>> GetActiveByRoleAsync(UserRole role) =>
        Task.FromResult(Users.Where(u => u.Role == role && u.IsActive).OrderBy(u => u.Name).ToList());

    public Task AddAsync(User user)
    {
        user.Id = _nextId++;
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => Task.CompletedTask;

    public User Seed(string name, string email, string passwordHash, UserRole role,
        bool requiresPasswordChange = false, bool isActive = true)
    {
        var user = new User
        {
            Id = _nextId++,
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            RequiresPasswordChange = requiresPasswordChange,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        Users.Add(user);
        return user;
    }
}
