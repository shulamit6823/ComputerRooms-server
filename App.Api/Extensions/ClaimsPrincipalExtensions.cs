using System.Security.Claims;
using App.Core.Enums;

namespace App.Api.Extensions;

/// <summary>שליפת המשתמש המחובר מתוך ה־Claims של ה־JWT</summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

    public static UserRole GetRole(this ClaimsPrincipal user) =>
        Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role));
}
