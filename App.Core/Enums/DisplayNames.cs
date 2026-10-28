namespace App.Core.Enums;

/// <summary>
/// שמות תצוגה בעברית - משמשים בחיפוש טקסט חופשי ובהמרת סוג הפנייה
/// ב־JSON (RequestTypeJsonConverter) וב־query string (RequestTypeModelBinder).
/// סוג פנייה חדש צריך להופיע כאן.
/// </summary>
public static class DisplayNames
{
    public static readonly IReadOnlyDictionary<RequestType, string> RequestTypes = new Dictionary<RequestType, string>
    {
        [RequestType.Hardware] = "חומרה",
        [RequestType.Software] = "תוכנה",
        [RequestType.Network] = "רשת",
        [RequestType.OperatingSystem] = "מערכת הפעלה",
        [RequestType.Installation] = "התקנה"
    };

    /// <summary>
    /// ממיר טקסט מהלקוח לסוג פנייה: קודם השם בעברית ("חומרה"), ולתאימות לאחור
    /// גם השם באנגלית ("Hardware", בלי תלות ברישיות) או הערך המספרי ("0").
    /// </summary>
    public static bool TryParseRequestType(string? value, out RequestType type)
    {
        var trimmed = value?.Trim();

        foreach (var (requestType, displayName) in RequestTypes)
        {
            if (displayName == trimmed)
            {
                type = requestType;
                return true;
            }
        }

        return Enum.TryParse(trimmed, ignoreCase: true, out type) && Enum.IsDefined(type);
    }
}
