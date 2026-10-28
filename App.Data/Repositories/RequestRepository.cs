using App.Core.DTOs.Requests;
using App.Core.Entities;
using App.Core.Enums;
using App.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace App.Data.Repositories;

public class RequestRepository : IRequestRepository
{
    private const string LikeEscape = "\\";

    private readonly AppDbContext _context;

    public RequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Request request)
    {
        await _context.Requests.AddAsync(request);
    }

    public Task<Request?> GetWithDetailsAsync(int id) =>
        _context.Requests
            .Include(r => r.Teacher)
            .Include(r => r.CurrentResponsible)
            .Include(r => r.Messages)
            .Include(r => r.TreatmentHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<Request>> GetByTeacherIdAsync(int teacherId) =>
        _context.Requests
            .AsNoTracking()
            .Where(r => r.TeacherId == teacherId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<List<Request>> GetByCurrentResponsibleIdAsync(int responsibleId) =>
        WithListIncludes(_context.Requests.AsNoTracking())
            .Where(r => r.CurrentResponsibleId == responsibleId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<List<Request>> GetAssignedWithTreatmentsAsync(int responsibleId) =>
        _context.Requests
            .Include(r => r.TreatmentHistory)
            .Where(r => r.CurrentResponsibleId == responsibleId)
            .ToListAsync();

    public Task<List<Request>> SearchAsync(RequestQueryDto query)
    {
        var requests = WithListIncludes(_context.Requests.AsNoTracking());

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var pattern = $"%{EscapeLike(search)}%";

            // סוג הפנייה נשמר כ־Enum, לכן מחפשים לפי השם בעברית או באנגלית
            var matchingTypes = DisplayNames.RequestTypes
                .Where(t => t.Value.Contains(search) ||
                            t.Key.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.Key)
                .ToList();

            // תו ה־Escape חייב להיות מוגדר במפורש - בלעדיו Npgsql שולח ESCAPE ''
            requests = requests.Where(r =>
                EF.Functions.ILike(r.Teacher.Name, pattern, LikeEscape) ||
                EF.Functions.ILike(r.Teacher.Email!, pattern, LikeEscape) ||
                EF.Functions.ILike(r.RoomName, pattern, LikeEscape) ||
                (r.ComputerNumber != null && EF.Functions.ILike(r.ComputerNumber, pattern, LikeEscape)) ||
                matchingTypes.Contains(r.RequestType) ||
                r.Messages.Any(m => EF.Functions.ILike(m.Description, pattern, LikeEscape)) ||
                r.TreatmentHistory.Any(t => EF.Functions.ILike(t.ResponsibleName, pattern, LikeEscape)));
        }

        if (query.Status.HasValue)
            requests = requests.Where(r => r.Status == query.Status.Value);

        if (query.RequestType.HasValue)
            requests = requests.Where(r => r.RequestType == query.RequestType.Value);

        if (query.ResponsibleId.HasValue)
            requests = requests.Where(r => r.CurrentResponsibleId == query.ResponsibleId.Value);

        // תאריך פתיחה נשמר ב־UTC; הסינון הוא לפי ימים שלמים בשעון המקומי
        if (query.CreatedFrom.HasValue)
        {
            var from = ToUtcStartOfDay(query.CreatedFrom.Value);
            requests = requests.Where(r => r.CreatedAt >= from);
        }

        if (query.CreatedTo.HasValue)
        {
            var toExclusive = ToUtcStartOfDay(query.CreatedTo.Value.AddDays(1));
            requests = requests.Where(r => r.CreatedAt < toExclusive);
        }

        if (query.DueFrom.HasValue)
        {
            var from = query.DueFrom.Value.Date;
            requests = requests.Where(r => r.DueDate >= from);
        }

        if (query.DueTo.HasValue)
        {
            var to = query.DueTo.Value.Date;
            requests = requests.Where(r => r.DueDate <= to);
        }

        requests = query.SortBy switch
        {
            RequestSortBy.Oldest => requests.OrderBy(r => r.CreatedAt),
            RequestSortBy.DueDateSoonest => requests.OrderBy(r => r.DueDate).ThenBy(r => r.CreatedAt),
            RequestSortBy.DueDateLatest => requests.OrderByDescending(r => r.DueDate).ThenBy(r => r.CreatedAt),
            _ => requests.OrderByDescending(r => r.CreatedAt)
        };

        return requests.ToListAsync();
    }

    public void Remove(Request request)
    {
        _context.Requests.Remove(request);
    }

    public Task SaveChangesAsync() => SaveChangesHelper.SaveAsync(_context);

    /// <summary>מה שצריך לרשימת פניות: מורה, אחראית נוכחית והתיאור המקורי בלבד</summary>
    private static IQueryable<Request> WithListIncludes(IQueryable<Request> requests) =>
        requests
            .Include(r => r.Teacher)
            .Include(r => r.CurrentResponsible)
            .Include(r => r.Messages.OrderBy(m => m.CreatedAt).Take(1));

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    private static DateTime ToUtcStartOfDay(DateTime date) =>
        DateTime.SpecifyKind(date.Date, DateTimeKind.Local).ToUniversalTime();
}
