using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestCachingController : ControllerBase
    {
        
        // GET, HEAD만 가능
        [HttpGet]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get()
        {
            
            await Task.Delay(1000);
            return Ok("Hello World");
            
        }
        
        [HttpGet("config")]
        public IActionResult TestConfiguration(IConfiguration config)
        {
            
            return Ok(config["Hello"]);
            
        }
        
    }
}
