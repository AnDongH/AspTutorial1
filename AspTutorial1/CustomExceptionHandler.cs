using System;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AspTutorial1;

public class CustomExceptionHandler : IExceptionHandler
{

    public CustomExceptionHandler()
    {
        
    }
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // using static System.Net.Mime.MediaTypeNames;
        httpContext.Response.ContentType = MediaTypeNames.Text.Plain;

        await httpContext.Response.WriteAsync("UseExceptionHandler: An exception was thrown.");
        
        if (exception is FileNotFoundException)
        {
            await httpContext.Response.WriteAsync("UseExceptionHandler: The file was not found.");
        }

        return false;
    }
}