using App.Core.DTOs.Requests;
using App.Core.Enums;

namespace App.Core.Interfaces.Services;

public interface IRequestService
{
    // מורה
    Task<TeacherRequestDetailsDto> CreateAsync(int teacherId, CreateRequestDto dto);
    Task<List<TeacherRequestListItemDto>> GetMyRequestsAsync(int teacherId);
    Task<TeacherRequestDetailsDto> GetForTeacherAsync(int requestId, int teacherId);
    Task<TeacherRequestDetailsDto> UpdateByTeacherAsync(int requestId, int teacherId, UpdateRequestDto dto);
    Task<TeacherRequestDetailsDto> ReopenAsync(int requestId, int teacherId);
    Task DeleteAsync(int requestId, int teacherId);

    // אחראית
    Task<List<RequestListItemDto>> GetAllAsync(RequestQueryDto query);
    Task<List<RequestListItemDto>> GetAssignedAsync(int responsibleId);
    Task<RequestDetailsDto> GetForResponsibleAsync(int requestId);
    Task<RequestDetailsDto> ChangeTypeAsync(int requestId, int responsibleId, ChangeRequestTypeDto dto);
    Task<RequestDetailsDto> TakeAsync(int requestId, int responsibleId);
    Task<RequestDetailsDto> CompleteAsync(int requestId, int responsibleId);
    Task<RequestDetailsDto> ChangeStatusAsync(int requestId, int responsibleId, RequestStatus status);
    Task<MessageDto> AddInternalNoteAsync(int requestId, int responsibleId, AddInternalNoteDto dto);

    // מורה ואחראית - שורה גלויה במערך התיאור
    Task<MessageDto> AddMessageAsync(int requestId, int userId, UserRole role, AddMessageDto dto);
}
