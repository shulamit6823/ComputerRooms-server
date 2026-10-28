using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>שורה ברשימת הפניות של האחראיות</summary>
public class RequestListItemDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = null!;
    public string TeacherEmail { get; set; } = null!;
    public string RoomName { get; set; } = null!;
    public string? ComputerNumber { get; set; }
    public RequestType RequestType { get; set; }

    /// <summary>התיאור המקורי - השורה הראשונה במערך התיאור</summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public RequestStatus Status { get; set; }
    public int? CurrentResponsibleId { get; set; }
    public string? CurrentResponsibleName { get; set; }
}
