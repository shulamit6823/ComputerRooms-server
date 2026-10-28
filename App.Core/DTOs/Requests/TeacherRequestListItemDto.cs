using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>שורה ברשימת הפניות של המורה - ללא פרטי האחראית</summary>
public class TeacherRequestListItemDto
{
    public int Id { get; set; }
    public string RoomName { get; set; } = null!;
    public string? ComputerNumber { get; set; }
    public RequestType RequestType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public RequestStatus Status { get; set; }
}
