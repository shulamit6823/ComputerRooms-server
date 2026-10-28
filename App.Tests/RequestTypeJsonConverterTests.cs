using System.Text.Json;
using System.Text.Json.Serialization;
using App.Api.Json;
using App.Core.DTOs.Requests;
using App.Core.Enums;
using Xunit;

namespace App.Tests;

public class RequestTypeJsonConverterTests
{
    // אותה הגדרה כמו ב־Program.cs
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new RequestTypeJsonConverter(), new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData("חומרה", RequestType.Hardware)]
    [InlineData("תוכנה", RequestType.Software)]
    [InlineData("רשת", RequestType.Network)]
    [InlineData("מערכת הפעלה", RequestType.OperatingSystem)]
    [InlineData("התקנה", RequestType.Installation)]
    [InlineData(" חומרה ", RequestType.Hardware)]
    public void Read_HebrewName_ReturnsInternalValue(string hebrew, RequestType expected)
    {
        var dto = JsonSerializer.Deserialize<CreateRequestDto>($"{{\"requestType\":\"{hebrew}\"}}", Options)!;

        Assert.Equal(expected, dto.RequestType);
    }

    [Theory]
    [InlineData("\"Hardware\"", RequestType.Hardware)]
    [InlineData("\"operatingSystem\"", RequestType.OperatingSystem)]
    [InlineData("\"1\"", RequestType.Software)]
    [InlineData("1", RequestType.Software)]
    public void Read_EnglishNameOrNumber_StillSupported(string json, RequestType expected)
    {
        var dto = JsonSerializer.Deserialize<CreateRequestDto>($"{{\"requestType\":{json}}}", Options)!;

        Assert.Equal(expected, dto.RequestType);
    }

    [Fact]
    public void Read_Null_StaysNullForRequiredValidation()
    {
        var dto = JsonSerializer.Deserialize<CreateRequestDto>("{\"requestType\":null}", Options)!;

        Assert.Null(dto.RequestType);
    }

    [Theory]
    [InlineData("\"מדפסת\"")]
    [InlineData("\"99\"")]
    [InlineData("99")]
    [InlineData("true")]
    public void Read_UnknownValue_Throws(string json)
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<CreateRequestDto>($"{{\"requestType\":{json}}}", Options));
    }

    [Theory]
    [InlineData(RequestType.Hardware, "חומרה")]
    [InlineData(RequestType.Software, "תוכנה")]
    [InlineData(RequestType.Network, "רשת")]
    [InlineData(RequestType.OperatingSystem, "מערכת הפעלה")]
    [InlineData(RequestType.Installation, "התקנה")]
    public void Write_ReturnsHebrewName(RequestType type, string expected)
    {
        var json = JsonSerializer.Serialize(new RequestListItemDto { RequestType = type }, Options);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal(expected, doc.RootElement.GetProperty("requestType").GetString());
    }

    [Fact]
    public void Write_OtherEnumsStayInEnglish()
    {
        var json = JsonSerializer.Serialize(new RequestListItemDto { Status = RequestStatus.Pending }, Options);

        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Pending", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public void EveryRequestTypeHasHebrewName()
    {
        foreach (var type in Enum.GetValues<RequestType>())
            Assert.True(DisplayNames.RequestTypes.ContainsKey(type), $"חסר שם בעברית ל־{type}");
    }
}
