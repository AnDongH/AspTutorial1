using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspTutorial1.Exceptions;

public class HttpResponseException : Exception
{
    public HttpResponseException(int statusCode, object? value = null) =>
        (StatusCode, Value) = (statusCode, value);

    public int StatusCode { get; }
    public object? Value { get; }
}

// 컨트롤러 단에서 동작하는 예외 필터
public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
{
    public int Order => int.MaxValue - 10;

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is HttpResponseException httpResponseException)
        {
            context.Result = new ObjectResult(httpResponseException.Value)
            {
                StatusCode = httpResponseException.StatusCode
            };
            
            // 이거 해줘야 서버에서 예외 발생 안하고, 예외 핸들러 미들웨어 동작 안함
            context.ExceptionHandled = true;
        }
    }
}