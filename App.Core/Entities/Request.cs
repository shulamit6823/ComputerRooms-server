using App.Core.Enums;

namespace App.Core.Entities;

public class Request
{
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public string RoomName { get; set; } = null!;
    public string? ComputerNumber { get; set; }
    public RequestType RequestType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// מתעדכן בכל שינוי בפנייה. מבטיח שכל פעולה תעדכן את שורת הפנייה
    /// וכך תעבור דרך בדיקת ה־Concurrency.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public int? CurrentResponsibleId { get; set; }
    public User? CurrentResponsible { get; set; }

    public ICollection<RequestMessage> Messages { get; set; } = new List<RequestMessage>();
    public ICollection<TreatmentHistory> TreatmentHistory { get; set; } = new List<TreatmentHistory>();
}
