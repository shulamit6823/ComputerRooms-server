using System.ComponentModel.DataAnnotations;
using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>שינוי סוג פנייה על ידי אחראית. תאריך היעד נערך רק על ידי המורה שפתחה את הפנייה.</summary>
public class ChangeRequestTypeDto
{
    [Required(ErrorMessage = "סוג פנייה הוא שדה חובה")]
    public RequestType? RequestType { get; set; }
}
