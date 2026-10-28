using System.Globalization;
using App.Api.Binding;
using App.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace App.Tests;

public class RequestTypeModelBinderTests
{
    private static async Task<DefaultModelBindingContext> BindAsync(string? queryValue, Type modelType)
    {
        var query = new Dictionary<string, StringValues>();
        if (queryValue != null)
            query["requestType"] = queryValue;

        var context = new DefaultModelBindingContext
        {
            ModelName = "requestType",
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(modelType),
            ModelState = new ModelStateDictionary(),
            ValueProvider = new QueryStringValueProvider(BindingSource.Query, new QueryCollection(query), CultureInfo.InvariantCulture)
        };

        await new RequestTypeModelBinder().BindModelAsync(context);
        return context;
    }

    [Theory]
    [InlineData("חומרה", RequestType.Hardware)]
    [InlineData("תוכנה", RequestType.Software)]
    [InlineData("רשת", RequestType.Network)]
    [InlineData("מערכת הפעלה", RequestType.OperatingSystem)]
    [InlineData("התקנה", RequestType.Installation)]
    [InlineData(" חומרה ", RequestType.Hardware)]
    public async Task Bind_HebrewName_ReturnsInternalValue(string hebrew, RequestType expected)
    {
        var context = await BindAsync(hebrew, typeof(RequestType?));

        Assert.True(context.Result.IsModelSet);
        Assert.Equal(expected, context.Result.Model);
        Assert.Equal(0, context.ModelState.ErrorCount);
    }

    [Theory]
    [InlineData("Hardware", RequestType.Hardware)]
    [InlineData("operatingsystem", RequestType.OperatingSystem)]
    [InlineData("1", RequestType.Software)]
    public async Task Bind_EnglishNameOrNumber_StillSupported(string value, RequestType expected)
    {
        var context = await BindAsync(value, typeof(RequestType?));

        Assert.Equal(expected, context.Result.Model);
        Assert.Equal(0, context.ModelState.ErrorCount);
    }

    [Theory]
    [InlineData("מדפסת")]
    [InlineData("99")]
    public async Task Bind_UnknownValue_AddsModelError(string value)
    {
        var context = await BindAsync(value, typeof(RequestType?));

        Assert.False(context.Result.IsModelSet);
        Assert.Equal(1, context.ModelState.ErrorCount);
    }

    [Fact]
    public async Task Bind_MissingValue_LeavesFilterEmpty()
    {
        var context = await BindAsync(null, typeof(RequestType?));

        Assert.False(context.Result.IsModelSet);
        Assert.Equal(0, context.ModelState.ErrorCount);
    }

    [Fact]
    public async Task Bind_EmptyValueOnNullable_MeansNoFilter()
    {
        var context = await BindAsync("", typeof(RequestType?));

        Assert.True(context.Result.IsModelSet);
        Assert.Null(context.Result.Model);
        Assert.Equal(0, context.ModelState.ErrorCount);
    }

    [Fact]
    public void Provider_HandlesRequestTypeAndNullableOnly()
    {
        var metadata = new EmptyModelMetadataProvider();
        var provider = new RequestTypeModelBinderProvider();

        Assert.IsType<RequestTypeModelBinder>(provider.GetBinder(new TestProviderContext(metadata.GetMetadataForType(typeof(RequestType)))));
        Assert.IsType<RequestTypeModelBinder>(provider.GetBinder(new TestProviderContext(metadata.GetMetadataForType(typeof(RequestType?)))));
        Assert.Null(provider.GetBinder(new TestProviderContext(metadata.GetMetadataForType(typeof(RequestStatus?)))));
    }

    private class TestProviderContext : ModelBinderProviderContext
    {
        public TestProviderContext(ModelMetadata metadata) => Metadata = metadata;

        public override BindingInfo BindingInfo => new();
        public override ModelMetadata Metadata { get; }
        public override IModelMetadataProvider MetadataProvider => new EmptyModelMetadataProvider();
        public override IModelBinder CreateBinder(ModelMetadata metadata) => throw new NotSupportedException();
    }
}
