namespace App.Core.DTOs.Auth;

public class AuthResponseDto
{
    /// <summary>null כאשר המשתמש חייב להחליף סיסמה לפני כניסה</summary>
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public UserDto User { get; set; } = null!;
}
