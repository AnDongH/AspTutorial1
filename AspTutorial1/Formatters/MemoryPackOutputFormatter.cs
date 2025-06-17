using System;
using System.Threading.Tasks;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace AspTutorial1.Formatters;

public class MemoryPackOutputFormatter : OutputFormatter
{
    private static readonly MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/x-memorypack");

    public MemoryPackOutputFormatter()
    {
        SupportedMediaTypes.Add(_mediaType);
    }

    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var objectType = context.Object?.GetType();
        
        return objectType != null && MemoryPackFormatterHelper.IsMemoryPackable(objectType);
    }

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var response = context.HttpContext.Response;

        var bytes = MemoryPackSerializer.Serialize(context.Object.GetType(), context.Object);
        await response.Body.WriteAsync(bytes, 0, bytes.Length);
    }
}