using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestRateLimiterController : ControllerBase
    {

        public TestRateLimiterController()
        {
            
        }
        
        /// <summary>
        /// test fixed rate limiter
        /// </summary>
        /// <returns></returns>
        [HttpGet("fixed")]
        [EnableRateLimiting("fixed")]
        public IActionResult GetFixed()
        {
            string ticks = (DateTime.Now.Ticks & 0x11111).ToString("00000");
            return Ok($"Hello World Fixed: {ticks}");
        }
        
        /// <summary>
        /// test sliding rate limiter
        /// </summary>
        /// <returns></returns>
        [HttpGet("sliding")]
        [EnableRateLimiting("sliding")]
        public IActionResult GetSliding()
        {
            string ticks = (DateTime.Now.Ticks & 0x11111).ToString("00000");
            return Ok($"Hello World Sliding: {ticks}");
        }
        
        /// <summary>
        /// test token bucket rate limiter
        /// </summary>
        /// <returns></returns>
        [HttpGet("token-bucket")]
        [EnableRateLimiting("token-bucket")]
        public IActionResult GetTokenBucket()
        {
            string ticks = (DateTime.Now.Ticks & 0x11111).ToString("00000");
            return Ok($"Hello World Token bucket: {ticks}");
        }
        
        /// <summary>
        /// test concurrency rate limiter
        /// </summary>
        /// <returns></returns>
        [HttpGet("concurrency")]
        [EnableRateLimiting("concurrency")]
        public IActionResult GetConcurrency()
        {
            string ticks = (DateTime.Now.Ticks & 0x11111).ToString("00000");
            return Ok($"Hello World Concurrency: {ticks}");
        }
        
    }
}
