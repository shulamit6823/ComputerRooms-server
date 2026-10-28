using App.Core.Enums;

namespace App.Core.Entities;

/// <summary>שורה אחת במערך התיאור / שיחה של הפנייה</summary>
public class RequestMessage
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public Request Request { get; set; } = null!;

    public int AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    /// <summary>שם הכותב בזמן הכתיבה - נשמר גם אם המשתמש ישתנה או יימחק</summary>
    public string AuthorName { get; set; } = null!;
    public UserRole AuthorRole { get; set; }

    public DateTime CreatedAt { get; set; }
    public string Description { get; set; } = null!;
    public MessageVisibility Visibility { get; set; }

    /// <summary>שלב הטיפול שבמהלכו נכתבה השורה. null אם נכתבה כשאף אחראית לא טיפלה בפנייה.</summary>
    public int? TreatmentHistoryId { get; set; }
    public TreatmentHistory? TreatmentHistory { get; set; }
}
