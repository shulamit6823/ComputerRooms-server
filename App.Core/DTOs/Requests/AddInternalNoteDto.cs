using System.ComponentModel.DataAnnotations;

namespace App.Core.DTOs.Requests;

/// <summary>הערה פנימית של אחראית - לא מוצגת למורה</summary>
public class AddInternalNoteDto
{
    [Required(ErrorMessage = "תוכן ההערה הוא שדה חובה")]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;
}
