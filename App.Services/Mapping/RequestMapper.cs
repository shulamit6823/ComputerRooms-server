using App.Core.DTOs.Requests;
using App.Core.Entities;
using App.Core.Enums;

namespace App.Services.Mapping;

public static class RequestMapper
{
    public static MessageDto ToDto(this RequestMessage message) => new()
    {
        Id = message.Id,
        CreatedAt = message.CreatedAt,
        AuthorName = message.AuthorName,
        AuthorRole = message.AuthorRole,
        Description = message.Description,
        Visibility = message.Visibility
    };

    public static TeacherRequestListItemDto ToTeacherListItemDto(this Request request) => new()
    {
        Id = request.Id,
        RoomName = request.RoomName,
        ComputerNumber = request.ComputerNumber,
        RequestType = request.RequestType,
        CreatedAt = request.CreatedAt,
        DueDate = request.DueDate,
        Status = request.Status
    };

    /// <summary>תצוגת מורה: בלי אחראית נוכחית, בלי היסטוריית טיפול ובלי הערות פנימיות</summary>
    public static TeacherRequestDetailsDto ToTeacherDetailsDto(this Request request) => new()
    {
        Id = request.Id,
        RoomName = request.RoomName,
        ComputerNumber = request.ComputerNumber,
        RequestType = request.RequestType,
        CreatedAt = request.CreatedAt,
        DueDate = request.DueDate,
        Status = request.Status,
        CompletedAt = request.CompletedAt,
        Messages = request.Messages
            .Where(m => m.Visibility == MessageVisibility.Public)
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.ToDto())
            .ToList()
    };

    public static RequestListItemDto ToListItemDto(this Request request) => new()
    {
        Id = request.Id,
        TeacherId = request.TeacherId,
        TeacherName = request.Teacher.Name,
        TeacherEmail = request.Teacher.Email!,
        RoomName = request.RoomName,
        ComputerNumber = request.ComputerNumber,
        RequestType = request.RequestType,
        Description = request.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault()?.Description,
        CreatedAt = request.CreatedAt,
        DueDate = request.DueDate,
        Status = request.Status,
        CurrentResponsibleId = request.CurrentResponsibleId,
        CurrentResponsibleName = request.CurrentResponsible?.Name
    };

    public static RequestDetailsDto ToDetailsDto(this Request request) => new()
    {
        Id = request.Id,
        TeacherId = request.TeacherId,
        TeacherName = request.Teacher.Name,
        TeacherEmail = request.Teacher.Email!,
        RoomName = request.RoomName,
        ComputerNumber = request.ComputerNumber,
        RequestType = request.RequestType,
        CreatedAt = request.CreatedAt,
        DueDate = request.DueDate,
        Status = request.Status,
        CompletedAt = request.CompletedAt,
        CurrentResponsibleId = request.CurrentResponsibleId,
        CurrentResponsibleName = request.CurrentResponsible?.Name,
        Messages = request.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.ToDto())
            .ToList(),
        TreatmentHistory = request.TreatmentHistory
            .OrderBy(t => t.StartedAt)
            .Select(t => new TreatmentHistoryDto
            {
                Id = t.Id,
                ResponsibleId = t.ResponsibleId,
                ResponsibleName = t.ResponsibleName,
                StartedAt = t.StartedAt,
                EndedAt = t.EndedAt,
                Status = t.Status,
                EndReason = t.EndReason,
                Messages = request.Messages
                    .Where(m => m.TreatmentHistoryId == t.Id)
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => m.ToDto())
                    .ToList()
            })
            .ToList()
    };
}
