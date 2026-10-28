using App.Core.Entities;

namespace App.Core.Interfaces.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
