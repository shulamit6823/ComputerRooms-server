using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "שם הוא שדה חובה")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "מייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת מייל לא תקינה")]
    [MaxLength(200)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    [MinLength(8, ErrorMessage = "סיסמה חייבת להכיל לפחות 8 תווים")]
    [MaxLength(100)]
    public string Password { get; set; } = null!;
}
