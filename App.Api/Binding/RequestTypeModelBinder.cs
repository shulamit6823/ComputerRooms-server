using App.Core.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace App.Api.Binding;

/// <summary>
/// סוג פנייה ב־query string / route: מתקבל בעברית (?requestType=חומרה) ומומר לערך הפנימי (Hardware).
/// לתאימות לאחור מתקבלים גם השם באנגלית והערך המספרי. ההמרה עצמה ב־DisplayNames.TryParseRequestType.
/// </summary>
public class RequestTypeModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var value = valueResult.FirstValue;

        // ערך ריק ב־RequestType? פירושו "בלי סינון", כמו בברירת המחדל של ASP.NET
        if (string.IsNullOrWhiteSpace(value) && bindingContext.ModelMetadata.IsReferenceOrNullableType)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        if (DisplayNames.TryParseRequestType(value, out var type))
            bindingContext.Result = ModelBindingResult.Success(type);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"סוג פנייה לא מוכר: {value?.Trim()}");

        return Task.CompletedTask;
    }
}

/// <summary>מפעיל את RequestTypeModelBinder עבור RequestType ו־RequestType?</summary>
public class RequestTypeModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.UnderlyingOrModelType == typeof(RequestType) ? new RequestTypeModelBinder() : null;
}
