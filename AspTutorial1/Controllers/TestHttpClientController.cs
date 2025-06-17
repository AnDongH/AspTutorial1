using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspTutorial1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestHttpClientController : ControllerBase
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TestHttpService _testHttpService;
        
        public TestHttpClientController(IHttpClientFactory httpClientFactory, TestHttpService testHttpService)
        {
            _httpClientFactory = httpClientFactory;
            _testHttpService = testHttpService;
        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> TestBasicUsageHttpClient()
        {
            var client = _httpClientFactory.CreateClient();
            
            using var m = await client.GetAsync("https://www.google.com");

            if (!m.IsSuccessStatusCode) return NotFound();
            
            var result = await m.Content.ReadAsStringAsync();
            
            return Ok(result);

        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> TestNamedHttpClient()
        {
            var client = _httpClientFactory.CreateClient("YouTube");
            
            using var m = await client.GetAsync("");

            if (!m.IsSuccessStatusCode) return NotFound();
            
            var result = await m.Content.ReadAsStringAsync();
            
            return Ok(result);

        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> TestTypedHttpClient()
        {

            try
            {
                var result = await _testHttpService.GetAsync("https://www.naver.com");
            
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }

        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> TestDelegateHttpClient()
        {
            var client = _httpClientFactory.CreateClient("DotNet");

            using var m = await client.GetAsync("");

            //if (!m.IsSuccessStatusCode) return NotFound();
            
            var result = await m.Content.ReadAsStringAsync();
            
            return Ok(result);

        }
        
        [HttpGet("[action]")]
        public async Task<IActionResult> TestHttpClientWithPolly()
        {
            var client = _httpClientFactory.CreateClient("PollyWaitAndRetry");
            
            using var m = await client.GetAsync("");

            if (!m.IsSuccessStatusCode) return NotFound();
            
            var result = await m.Content.ReadAsStringAsync();
            
            return Ok(result);

        }
        
    }
}
