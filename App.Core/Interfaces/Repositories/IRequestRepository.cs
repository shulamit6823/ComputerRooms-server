using App.Core.DTOs.Requests;
using App.Core.Entities;

namespace App.Core.Interfaces.Repositories;

public interface IRequestRepository
{
    Task AddAsync(Request request);

    /// <summary>פנייה עם המורה, האחראית הנוכחית, מערך התיאור והיסטוריית הטיפול</summary>
    Task<Request?> GetWithDetailsAsync(int id);

    Task<List<Request>> GetByTeacherIdAsync(int teacherId);
    Task<List<Request>> GetByCurrentResponsibleIdAsync(int responsibleId);

    /// <summary>חיפוש, סינון ומיון - השאילתה נבנית במסד הנתונים</summary>
    Task<List<Request>> SearchAsync(RequestQueryDto query);

    /// <summary>פניות שהאחראית משויכת אליהן, כולל היסטוריית הטיפול שלהן</summary>
    Task<List<Request>> GetAssignedWithTreatmentsAsync(int responsibleId);

    void Remove(Request request);

    /// <summary>
    /// שומר את כל השינויים. אם הפנייה שונתה במקביל על ידי משתמש אחר
    /// נזרק ConflictException.
    /// </summary>
    Task SaveChangesAsync();
}
