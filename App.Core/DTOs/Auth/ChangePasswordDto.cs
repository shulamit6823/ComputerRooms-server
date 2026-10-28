using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Auth;

/// <summary>החלפת הסיסמה הזמנית בכניסה הראשונה</summary>
public class ChangePasswordDto
{
    [Required(ErrorMessage = "מייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת מייל לא תקינה")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "הסיסמה הנוכחית היא שדה חובה")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "סיסמה חדשה היא שדה חובה")]
    [MinLength(8, ErrorMessage = "סיסמה חייבת להכיל לפחות 8 תווים")]
    [MaxLength(100)]
    public string NewPassword { get; set; } = null!;
}
