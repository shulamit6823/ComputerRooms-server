using App.Core.DTOs.Manager;

namespace App.Core.Interfaces.Services;

public interface IManagerService
{
    Task<List<ResponsibleDto>> GetResponsiblesAsync();
    Task<ResponsibleDto> CreateResponsibleAsync(CreateResponsibleDto dto);
    Task DeleteResponsibleAsync(int responsibleId);

    /// <summary>יוצר את המנהל הראשוני אם עדיין אין מנהל במערכת (Seed)</summary>
    Task EnsureInitialManagerAsync();
}
