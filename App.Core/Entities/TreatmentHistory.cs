using App.Core.Enums;

namespace App.Core.Entities;

/// <summary>שלב טיפול אחד של אחראית בפנייה</summary>
public class TreatmentHistory
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public Request Request { get; set; } = null!;

    public int ResponsibleId { get; set; }
    public User Responsible { get; set; } = null!;

    /// <summary>שם האחראית בזמן הטיפול</summary>
    public string ResponsibleName { get; set; } = null!;

    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    /// <summary>סטטוס הטיפול בשלב זה</summary>
    public RequestStatus Status { get; set; }
    public TreatmentEndReason? EndReason { get; set; }

    /// <summary>ההערות שנכתבו במהלך שלב הטיפול (פנימיות, למורה ומהמורה)</summary>
    public ICollection<RequestMessage> Messages { get; set; } = new List<RequestMessage>();
}
