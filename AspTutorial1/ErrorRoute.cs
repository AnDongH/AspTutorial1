using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspTutorial1;

// 전역 오류 처리 전용 컨트롤러
[ApiExplorerSettings(IgnoreApi = true)] // Swagger에서 숨김 처리
[Route("")]
public class ErrorController : ControllerBase
{
    
    [Route("error")]
    public IActionResult HandleError()
    {
        return Content("error!!!");
    }
    
}