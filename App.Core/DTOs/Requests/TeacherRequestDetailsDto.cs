using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>פרטי פנייה כפי שהמורה רואה אותם - ללא אחראית נוכחית וללא הערות פנימיות</summary>
public class TeacherRequestDetailsDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = null!;
    public string? ComputerNumber { get; set; }
    public RequestType RequestType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}
