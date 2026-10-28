using App.Core.DTOs.Manager;
using App.Core.Entities;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces.Repositories;
using App.Core.Interfaces.Services;
using App.Services.Mapping;
using App.Services.Settings;
using Microsoft.Extensions.Options;

namespace App.Services;

public class ManagerService : IManagerService
{
    private const string TemporaryPassword = "12345678";

    private readonly IUserRepository _userRepository;
    private readonly IRequestRepository _requestRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly InitialManagerSettings _initialManager;

    public ManagerService(
        IUserRepository userRepository,
        IRequestRepository requestRepository,
        IPasswordHasher passwordHasher,
        IOptions<InitialManagerSettings> initialManager)
    {
        _userRepository = userRepository;
        _requestRepository = requestRepository;
        _passwordHasher = passwordHasher;
        _initialManager = initialManager.Value;
    }

    public async Task<List<ResponsibleDto>> GetResponsiblesAsync()
    {
        var responsibles = await _userRepository.GetActiveByRoleAsync(UserRole.Responsible);
        return responsibles.Select(u => u.ToResponsibleDto()).ToList();
    }

    public async Task<ResponsibleDto> CreateResponsibleAsync(CreateResponsibleDto dto)
    {
        // השם אינו ייחודי. רק המייל נבדק.
        var name = dto.Name.Trim();

        // מייל של אחראית שנמחקה (לא פעילה) מותר לשימוש חוזר. הרשומה הישנה נשארת כמו שהיא.
        var email = EmailNormalizer.Normalize(dto.Email);
        if (await _userRepository.ActiveEmailExistsAsync(email))
            throw new ConflictException("כתובת המייל כבר קיימת במערכת.");

        var responsible = new User
        {
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(TemporaryPassword),
            RequiresPasswordChange = true,
            IsActive = true,
            Role = UserRole.Responsible,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(responsible);
        await _userRepository.SaveChangesAsync();

        return responsible.ToResponsibleDto();
    }

    /// <summary>
    /// מחיקה רכה: האחראית מסומנת כלא פעילה. הפניות שהיו אצלה נשארות במצבן
    /// (כולל "בטיפול") אבל בלי אחראית נוכחית, וההיסטוריה שלה נשמרת.
    /// </summary>
    public async Task DeleteResponsibleAsync(int responsibleId)
    {
        var responsible = await _userRepository.GetByIdAsync(responsibleId);
        if (responsible == null || responsible.Role != UserRole.Responsible || !responsible.IsActive)
            throw new NotFoundException("האחראית לא נמצאה.");

        var now = DateTime.UtcNow;
        responsible.IsActive = false;

        var requests = await _requestRepository.GetAssignedWithTreatmentsAsync(responsibleId);
        foreach (var request in requests)
        {
            var openTreatment = request.TreatmentHistory
                .FirstOrDefault(t => t.EndedAt == null && t.ResponsibleId == responsibleId);
            if (openTreatment != null)
            {
                openTreatment.EndedAt = now;
                openTreatment.EndReason = TreatmentEndReason.ResponsibleDeleted;
            }

            request.CurrentResponsibleId = null;
            request.UpdatedAt = now;
        }

        // שני ה־Repositories עובדים על אותו DbContext, לכן שמירה אחת שומרת הכל יחד
        await _requestRepository.SaveChangesAsync();
    }

    public async Task EnsureInitialManagerAsync()
    {
        var email = EmailNormalizer.Normalize(_initialManager.Email);

        var existingManager = await _userRepository.GetByRoleAsync(UserRole.Manager);
        if (existingManager != null)
        {
            // מנהל שנוצר לפני המעבר להתחברות לפי מייל - מקבל את המייל מההגדרות
            if (existingManager.Email == null)
            {
                existingManager.Email = email;
                await _userRepository.SaveChangesAsync();
            }
            return;
        }

        var manager = new User
        {
            Name = _initialManager.Name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(_initialManager.Password),
            RequiresPasswordChange = false,
            IsActive = true,
            Role = UserRole.Manager,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(manager);
        await _userRepository.SaveChangesAsync();
    }
}
