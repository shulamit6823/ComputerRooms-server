using System.ComponentModel.DataAnnotations;
using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>
/// עריכת פנייה על ידי המורה שפתחה אותה: סוג ותאריך יעד.
/// חדר, מחשב ותיאור לא נערכים - תוספות נעשות כשורה חדשה במערך התיאור.
/// </summary>
public class UpdateRequestDto
{
    [Required(ErrorMessage = "סוג פנייה הוא שדה חובה")]
    public RequestType? RequestType { get; set; }

    [Required(ErrorMessage = "תאריך יעד הוא שדה חובה")]
    public DateTime? DueDate { get; set; }
}
