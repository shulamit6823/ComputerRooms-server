using App.Core.Entities;
using App.Core.Enums;

namespace App.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    /// <summary>המשתמש הפעיל עם המייל הזה. המייל ייחודי בין המשתמשים הפעילים, לכן יש לכל היותר אחד.</summary>
    Task<User?> GetActiveByEmailAsync(string email);
    Task<User?> GetByRoleAsync(UserRole role);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> ActiveEmailExistsAsync(string email);
    Task<List<User>> GetActiveByRoleAsync(UserRole role);
    Task AddAsync(User user);

    /// <summary>שומר את כל השינויים ב־DbContext המשותף של ה־Request הנוכחי</summary>
    Task SaveChangesAsync();
}
