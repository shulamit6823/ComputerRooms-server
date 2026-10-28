using System.ComponentModel.DataAnnotations;
using App.Core.Enums;

namespace App.Core.DTOs.Requests;

public class CreateRequestDto
{
    [Required(ErrorMessage = "שם חדר הוא שדה חובה")]
    [MaxLength(100)]
    public string RoomName { get; set; } = null!;

    [MaxLength(50)]
    public string? ComputerNumber { get; set; }

    [Required(ErrorMessage = "סוג פנייה הוא שדה חובה")]
    public RequestType? RequestType { get; set; }

    [Required(ErrorMessage = "תיאור הוא שדה חובה")]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "תאריך יעד הוא שדה חובה")]
    public DateTime? DueDate { get; set; }
}
