using App.Core.Enums;

namespace App.Api.Authorization;

/// <summary>שמות התפקידים לשימוש ב־[Authorize(Roles = ...)]</summary>
public static class Roles
{
    public const string Teacher = nameof(UserRole.Teacher);
    public const string Responsible = nameof(UserRole.Responsible);
    public const string Manager = nameof(UserRole.Manager);
    public const string TeacherOrResponsible = Teacher + "," + Responsible;
}
