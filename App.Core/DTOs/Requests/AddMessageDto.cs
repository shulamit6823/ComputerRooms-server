using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Requests;

/// <summary>שורה חדשה במערך התיאור, גלויה למורה</summary>
public class AddMessageDto
{
    [Required(ErrorMessage = "תיאור הוא שדה חובה")]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;
}
