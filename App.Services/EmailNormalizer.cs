namespace App.Services;

internal static class EmailNormalizer
{
    /// <summary>מיילים נשמרים באותיות קטנות כדי שההשוואה לא תהיה תלויה ברישיות</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
