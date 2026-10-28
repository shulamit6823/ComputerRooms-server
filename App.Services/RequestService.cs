using App.Core.DTOs.Requests;
using App.Core.Entities;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Core.Interfaces.Services;
using App.Services.Mapping;

namespace App.Services;

public class RequestService : IRequestService
{
    private readonly IRequestRepository _requestRepository;
    private readonly IUserRepository _userRepository;

    public RequestService(IRequestRepository requestRepository, IUserRepository userRepository)
    {
        _requestRepository = requestRepository;
        _userRepository = userRepository;
    }

    // ---------- מורה ----------

    public async Task<TeacherRequestDetailsDto> CreateAsync(int teacherId, CreateRequestDto dto)
    {
        var teacher = await GetActiveUserAsync(teacherId);
        var now = DateTime.UtcNow;

        var request = new Request
        {
            TeacherId = teacher.Id,
            Teacher = teacher,
            RoomName = dto.RoomName.Trim(),
            ComputerNumber = string.IsNullOrWhiteSpace(dto.ComputerNumber) ? null : dto.ComputerNumber.Trim(),
            RequestType = dto.RequestType!.Value,
            CreatedAt = now,
            UpdatedAt = now,
            DueDate = ValidateDueDate(dto.DueDate!.Value),
            Status = RequestStatus.Pending
        };

        // התיאור המקורי הוא השורה הראשונה במערך התיאור
        request.Messages.Add(new RequestMessage
        {
            AuthorUserId = teacher.Id,
            AuthorName = teacher.Name,
            AuthorRole = UserRole.Teacher,
            CreatedAt = now,
            Description = dto.Description.Trim(),
            Visibility = MessageVisibility.Public
        });

        await _requestRepository.AddAsync(request);
        await _requestRepository.SaveChangesAsync();

        return request.ToTeacherDetailsDto();
    }

    public async Task<List<TeacherRequestListItemDto>> GetMyRequestsAsync(int teacherId)
    {
        var requests = await _requestRepository.GetByTeacherIdAsync(teacherId);
        return requests.Select(r => r.ToTeacherListItemDto()).ToList();
    }

    public async Task<TeacherRequestDetailsDto> GetForTeacherAsync(int requestId, int teacherId)
    {
        var request = await GetOwnedRequestAsync(requestId, teacherId);
        return request.ToTeacherDetailsDto();
    }

    public async Task<TeacherRequestDetailsDto> UpdateByTeacherAsync(int requestId, int teacherId, UpdateRequestDto dto)
    {
        await GetActiveUserAsync(teacherId);
        var request = await GetOwnedRequestAsync(requestId, teacherId);

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("ניתן לערוך פנייה רק כאשר היא ממתינה לטיפול.");

        // תאריך היעד משתנה רק כאן - על ידי המורה שפתחה את הפנייה
        request.RequestType = dto.RequestType!.Value;
        request.DueDate = ValidateDueDate(dto.DueDate!.Value);
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.SaveChangesAsync();

        return request.ToTeacherDetailsDto();
    }

    /// <summary>"הסתיימה" → "ממתינה לטיפול". האחראית הנוכחית מתאפסת, ההיסטוריה נשמרת.</summary>
    public async Task<TeacherRequestDetailsDto> ReopenAsync(int requestId, int teacherId)
    {
        await GetActiveUserAsync(teacherId);
        var request = await GetOwnedRequestAsync(requestId, teacherId);

        if (request.Status != RequestStatus.Finished)
            throw new BusinessRuleException("ניתן להחזיר להמתנה רק פנייה שהסתיימה.");

        ReturnToPending(request, DateTime.UtcNow);
        await _requestRepository.SaveChangesAsync();

        return request.ToTeacherDetailsDto();
    }

    public async Task DeleteAsync(int requestId, int teacherId)
    {
        await GetActiveUserAsync(teacherId);
        var request = await GetOwnedRequestAsync(requestId, teacherId);

        if (request.TreatmentHistory.Any())
            throw new BusinessRuleException("לא ניתן למחוק פנייה שכבר נלקחה לטיפול.");

        _requestRepository.Remove(request);
        await _requestRepository.SaveChangesAsync();
    }

    // ---------- אחראית ----------

    public async Task<List<RequestListItemDto>> GetAllAsync(RequestQueryDto query)
    {
        var requests = await _requestRepository.SearchAsync(query);
        return requests.Select(r => r.ToListItemDto()).ToList();
    }

    public async Task<List<RequestListItemDto>> GetAssignedAsync(int responsibleId)
    {
        var requests = await _requestRepository.GetByCurrentResponsibleIdAsync(responsibleId);
        return requests.Select(r => r.ToListItemDto()).ToList();
    }

    public async Task<RequestDetailsDto> GetForResponsibleAsync(int requestId)
    {
        var request = await GetRequestAsync(requestId);
        return request.ToDetailsDto();
    }

    /// <summary>אחראית משנה רק את סוג הפנייה - לא את תאריך היעד</summary>
    public async Task<RequestDetailsDto> ChangeTypeAsync(int requestId, int responsibleId, ChangeRequestTypeDto dto)
    {
        await GetActiveUserAsync(responsibleId);
        var request = await GetRequestAsync(requestId);

        request.RequestType = dto.RequestType!.Value;
        request.UpdatedAt = DateTime.UtcNow;
        await _requestRepository.SaveChangesAsync();

        return request.ToDetailsDto();
    }

    /// <summary>
    /// לקיחת פנייה שממתינה או שכבר בטיפול (גם אצל אחראית אחרת).
    /// אם שתי אחראיות לוקחות במקביל - השנייה מקבלת Conflict (בדיקת xmin במסד).
    /// </summary>
    public async Task<RequestDetailsDto> TakeAsync(int requestId, int responsibleId)
    {
        var responsible = await GetActiveUserAsync(responsibleId);
        var request = await GetRequestAsync(requestId);

        if (request.Status == RequestStatus.Finished)
            throw new BusinessRuleException("הפנייה הסתיימה. כדי לטפל בה שוב יש לשנות את הסטטוס שלה.");

        StartTreatment(request, responsible, DateTime.UtcNow);
        await _requestRepository.SaveChangesAsync();

        return request.ToDetailsDto();
    }

    public async Task<RequestDetailsDto> CompleteAsync(int requestId, int responsibleId)
    {
        var responsible = await GetActiveUserAsync(responsibleId);
        var request = await GetRequestAsync(requestId);

        Complete(request, responsible, DateTime.UtcNow);
        await _requestRepository.SaveChangesAsync();

        return request.ToDetailsDto();
    }

    /// <summary>כל אחראית רשאית להעביר את הפנייה לכל סטטוס</summary>
    public async Task<RequestDetailsDto> ChangeStatusAsync(int requestId, int responsibleId, RequestStatus status)
    {
        var responsible = await GetActiveUserAsync(responsibleId);
        var request = await GetRequestAsync(requestId);
        var now = DateTime.UtcNow;

        switch (status)
        {
            case RequestStatus.InProgress:
                StartTreatment(request, responsible, now);
                break;
            case RequestStatus.Finished:
                Complete(request, responsible, now);
                break;
            case RequestStatus.Pending:
                if (request.Status == RequestStatus.Pending)
                    throw new BusinessRuleException("הפנייה כבר ממתינה לטיפול.");
                ReturnToPending(request, now);
                break;
        }

        await _requestRepository.SaveChangesAsync();
        return request.ToDetailsDto();
    }

    public async Task<MessageDto> AddInternalNoteAsync(int requestId, int responsibleId, AddInternalNoteDto dto)
    {
        var responsible = await GetActiveUserAsync(responsibleId);
        var request = await GetRequestAsync(requestId);

        var message = AddMessage(request, responsible, dto.Description, MessageVisibility.Internal);
        await _requestRepository.SaveChangesAsync();

        return message.ToDto();
    }

    // ---------- מורה ואחראית ----------

    public async Task<MessageDto> AddMessageAsync(int requestId, int userId, UserRole role, AddMessageDto dto)
    {
        var author = await GetActiveUserAsync(userId);
        var request = role == UserRole.Teacher
            ? await GetOwnedRequestAsync(requestId, userId)
            : await GetRequestAsync(requestId);

        var message = AddMessage(request, author, dto.Description, MessageVisibility.Public);
        await _requestRepository.SaveChangesAsync();

        return message.ToDto();
    }

    // ---------- לוגיקה משותפת ----------

    private void StartTreatment(Request request, User responsible, DateTime now)
    {
        if (request.Status == RequestStatus.InProgress && request.CurrentResponsibleId == responsible.Id)
            throw new BusinessRuleException("הפנייה כבר בטיפולך.");

        // הטיפול הקודם נסגר אבל נשאר בהיסטוריה
        CloseOpenTreatment(request, now, TreatmentEndReason.TakenByAnother);

        request.CurrentResponsibleId = responsible.Id;
        request.CurrentResponsible = responsible;
        request.Status = RequestStatus.InProgress;
        request.CompletedAt = null;
        request.UpdatedAt = now;

        request.TreatmentHistory.Add(new TreatmentHistory
        {
            ResponsibleId = responsible.Id,
            ResponsibleName = responsible.Name,
            StartedAt = now,
            Status = RequestStatus.InProgress
        });
    }

    /// <summary>
    /// סיום טיפול. אם הפנייה לא נמצאת כרגע בטיפול של האחראית המסיימת,
    /// נפתח עבורה שלב טיפול כדי שההיסטוריה תראה מי סיימה את הפנייה.
    /// הפנייה נשארת משויכת לאחראית האחרונה.
    /// </summary>
    private void Complete(Request request, User responsible, DateTime now)
    {
        if (request.Status == RequestStatus.Finished)
            throw new BusinessRuleException("הפנייה כבר הסתיימה.");

        if (GetOpenTreatment(request)?.ResponsibleId != responsible.Id)
            StartTreatment(request, responsible, now);

        var treatment = GetOpenTreatment(request)!;
        treatment.EndedAt = now;
        treatment.Status = RequestStatus.Finished;
        treatment.EndReason = TreatmentEndReason.Completed;

        request.Status = RequestStatus.Finished;
        request.CompletedAt = now;
        request.UpdatedAt = now;
    }

    private static void ReturnToPending(Request request, DateTime now)
    {
        CloseOpenTreatment(request, now, TreatmentEndReason.ReturnedToPending);

        request.Status = RequestStatus.Pending;
        request.CurrentResponsibleId = null;
        request.CurrentResponsible = null;
        request.CompletedAt = null;
        request.UpdatedAt = now;
    }

    private static RequestMessage AddMessage(Request request, User author, string description, MessageVisibility visibility)
    {
        var message = new RequestMessage
        {
            AuthorUserId = author.Id,
            AuthorName = author.Name,
            AuthorRole = author.Role,
            CreatedAt = DateTime.UtcNow,
            Description = description.Trim(),
            Visibility = visibility,
            // ההערה משויכת לשלב הטיפול הפעיל (אם יש)
            TreatmentHistory = GetOpenTreatment(request)
        };

        request.Messages.Add(message);
        return message;
    }

    private static TreatmentHistory? GetOpenTreatment(Request request) =>
        request.TreatmentHistory.FirstOrDefault(t => t.EndedAt == null);

    private static void CloseOpenTreatment(Request request, DateTime now, TreatmentEndReason reason)
    {
        var open = GetOpenTreatment(request);
        if (open == null)
            return;

        open.EndedAt = now;
        open.EndReason = reason;
    }

    private static DateTime ValidateDueDate(DateTime dueDate)
    {
        var date = DateTime.SpecifyKind(dueDate.Date, DateTimeKind.Unspecified);
        if (date < DateTime.Today)
            throw new BusinessRuleException("לא ניתן לבחור תאריך יעד שכבר עבר.");

        return date;
    }

    private async Task<User> GetActiveUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedException("המשתמש אינו פעיל במערכת.");

        return user;
    }

    private async Task<Request> GetRequestAsync(int requestId)
    {
        var request = await _requestRepository.GetWithDetailsAsync(requestId);
        if (request == null)
            throw new NotFoundException("הפנייה לא נמצאה.");

        return request;
    }

    private async Task<Request> GetOwnedRequestAsync(int requestId, int teacherId)
    {
        var request = await GetRequestAsync(requestId);
        if (request.TeacherId != teacherId)
            throw new ForbiddenException("אין לך הרשאה לפנייה זו.");

        return request;
    }
}
