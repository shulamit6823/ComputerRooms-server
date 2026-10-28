using App.Core.Enums;

namespace App.Core.DTOs.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
}
