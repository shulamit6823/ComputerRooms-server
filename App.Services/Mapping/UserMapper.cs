using App.Core.DTOs.Auth;
using App.Core.DTOs.Manager;
using App.Core.Entities;

namespace App.Services.Mapping;

public static class UserMapper
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role
    };

    public static ResponsibleDto ToResponsibleDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email!,
        RequiresPasswordChange = user.RequiresPasswordChange,
        CreatedAt = user.CreatedAt
    };
}
