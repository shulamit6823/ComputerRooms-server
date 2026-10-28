using App.Core.Enums;

namespace App.Core.DTOs.Requests;

public class TreatmentHistoryDto
{
    public int Id { get; set; }
    public int ResponsibleId { get; set; }
    public string ResponsibleName { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public RequestStatus Status { get; set; }
    public TreatmentEndReason? EndReason { get; set; }

    /// <summary>כל ההערות שנכתבו בשלב זה: פנימיות, למורה ומהמורה</summary>
    public List<MessageDto> Messages { get; set; } = new();
}
