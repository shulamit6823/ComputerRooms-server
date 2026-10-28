using System.Text.Json;
using System.Text.Json.Serialization;
using App.Core.Enums;

namespace App.Api.Json;

/// <summary>
/// סוג פנייה ב־JSON: נשלח לפרונט בעברית ("חומרה") ומתקבל ממנו בעברית.
/// בתוך המערכת ובמסד הנתונים הערך נשאר באנגלית (Hardware).
/// לתאימות לאחור מתקבלים גם השם באנגלית והערך המספרי, כמו ב־JsonStringEnumConverter.
/// </summary>
public class RequestTypeJsonConverter : JsonConverter<RequestType>
{
    public override RequestType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number)
            && Enum.IsDefined((RequestType)number))
            return (RequestType)number;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("סוג פנייה לא מוכר.");

        var value = reader.GetString();
        if (DisplayNames.TryParseRequestType(value, out var type))
            return type;

        throw new JsonException($"סוג פנייה לא מוכר: {value?.Trim()}");
    }

    public override void Write(Utf8JsonWriter writer, RequestType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(DisplayNames.RequestTypes.TryGetValue(value, out var displayName)
            ? displayName
            : value.ToString());
}
