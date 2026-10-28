using App.Core.Entities;
using App.Core.Enums;
using App.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(int id) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetActiveByEmailAsync(string email) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

    public Task<User?> GetByRoleAsync(UserRole role) =>
        _context.Users.OrderBy(u => u.Id).FirstOrDefaultAsync(u => u.Role == role);

    public Task<bool> EmailExistsAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email == email);

    public Task<bool> ActiveEmailExistsAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email == email && u.IsActive);

    public Task<List<User>> GetActiveByRoleAsync(UserRole role) =>
        _context.Users
            .AsNoTracking()
            .Where(u => u.Role == role && u.IsActive)
            .OrderBy(u => u.Name)
            .ToListAsync();

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task SaveChangesAsync() => SaveChangesHelper.SaveAsync(_context);
}
