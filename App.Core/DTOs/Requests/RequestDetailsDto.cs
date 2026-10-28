using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>פרטי פנייה מלאים עבור אחראית</summary>
public class RequestDetailsDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public string TeacherEmail { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public string? ComputerNumber { get; set; }
    public RequestType RequestType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CurrentResponsibleId { get; set; }
    public string? CurrentResponsibleName { get; set; }

    /// <summary>כל מערך התיאור, כולל הערות פנימיות</summary>
    public List<MessageDto> Messages { get; set; } = new();

    public List<TreatmentHistoryDto> TreatmentHistory { get; set; } = new();
}
