using AspTutorial1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemoryPackFormatterController : ControllerBase
    {

        [HttpGet("[action]")]
        public MemTestDTO Get()
        {

            var res = new MemTestDTO()
            {
                Street = "ilsan",
                City = "Goyang",
                Country = "Korea",
                PostalCode = "12345"
            };
            
            return res;
        }
        
        [HttpPost("[action]")]
        public ActionResult<MemTestDTO> Post(MemTestDTO dto)
        {
            return CreatedAtAction("Post", dto);
        }
        
    }
}
