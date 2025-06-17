using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRoutingController : ControllerBase
    {

        private readonly LinkGenerator _linkGenerator;
        
        public TestRoutingController(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }
        
        // 내 API의 라우팅 패스를 가져오는 테스트
        [HttpGet("[action]/[controller]")]
        public IActionResult TestGetLinkGenerator()
        {
            return Content(_linkGenerator.GetPathByAction("GetSliding", "TestRateLimiter"));
        }
        
        // 내 API의 라우팅 패스를 가져오는 테스트
        [HttpGet("[action]")]
        public IActionResult TestRouting()
        {
            return StatusCode(200);
        }
        
        // 내 API의 라우팅 패스를 가져오는 테스트
        [HttpGet("[action]/{id}")]
        public IActionResult TestRouting(int id)
        {
            return Content(id.ToString());
        }
        
    }
}
