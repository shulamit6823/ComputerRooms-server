using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Auth;

/// <summary>התחברות לכל סוגי המשתמשים: מייל + סיסמה</summary>
public class LoginDto
{
    [Required(ErrorMessage = "מייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת מייל לא תקינה")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "סיסמה היא שדה חובה")]
    public string Password { get; set; } = null!;
}
