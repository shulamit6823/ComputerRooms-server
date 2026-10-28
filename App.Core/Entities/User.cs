using App.Core.Enums;

namespace App.Core.Entities;

public class User
{
    public int Id { get; set; }
    /// <summary>שם המשתמש - ייחודי, משמש להתחברות</summary>
    public string Name { get; set; } = null!;

    /// <summary>משמש להתחברות. המנהל מקבל את המייל שלו מההגדרות (InitialManager:Email).</summary>
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = null!;
    public bool RequiresPasswordChange { get; set; }
    public bool IsActive { get; set; } = true;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>פניות שהמורה פתחה</summary>
    public ICollection<Request> Requests { get; set; } = new List<Request>();
}
