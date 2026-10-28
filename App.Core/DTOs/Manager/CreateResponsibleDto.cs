using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Manager;

public class CreateResponsibleDto
{
    [Required(ErrorMessage = "שם הוא שדה חובה")]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "מייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת מייל לא תקינה")]
    [MaxLength(200)]
    public string Email { get; set; } = null!;
}
