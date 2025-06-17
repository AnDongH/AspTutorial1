using System;
using System.IO;
using System.Threading.Tasks;
using AspTutorial1.Models;
using MemoryPack;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using ArgumentNullException = System.ArgumentNullException;

namespace AspTutorial1.Formatters;

public class MemoryPackInputFormatter : InputFormatter
{
    private static readonly MediaTypeHeaderValue _mediaType = MediaTypeHeaderValue.Parse("application/x-memorypack");
    
    public MemoryPackInputFormatter()
    {
        SupportedMediaTypes.Add(_mediaType);
    }

    public override bool CanRead(InputFormatterContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var type = context.ModelType;
        
        return MemoryPackFormatterHelper.IsMemoryPackable(type);
    }
    
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var request = context.HttpContext.Request;
        
        using (var memoryStream = new MemoryStream())
        {
            await request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            
            try
            {
                var model = MemoryPackSerializer.Deserialize(context.ModelType, memoryStream.ToArray());
                return await InputFormatterResult.SuccessAsync(model);
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError("MemoryPack", $"MemoryPack deserialization error: {ex.Message}");
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}