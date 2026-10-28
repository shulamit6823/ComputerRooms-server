using App.Core.Enums;

namespace App.Core.DTOs.Requests;

/// <summary>פרמטרים לחיפוש, סינון ומיון ברשימת הפניות של האחראיות</summary>
public class RequestQueryDto
{
    public string? Search { get; set; }
    public RequestStatus? Status { get; set; }
    public RequestType? RequestType { get; set; }
    public int? ResponsibleId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? DueFrom { get; set; }
    public DateTime? DueTo { get; set; }
    public RequestSortBy SortBy { get; set; } = RequestSortBy.Newest;
}
