using System;
using AspTutorial1.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestExceptionController : ControllerBase
    {

        public TestExceptionController()
        {
            
        }

        [HttpGet("[action]")]
        public void ExceptionGet()
        {
            throw new Exception("sdsdsdsds");
        }
        
        [HttpGet("[action]")]
        public void HttpResponsExceptionGet()
        {
            throw new HttpResponseException(StatusCodes.Status400BadRequest, new { message = "This is a bad request" });
        }

        [HttpGet("[action]")]
        public void TestExeptionGet()
        {
            try
            {
                throw new TestSubExeption();
            }
            catch (TestSubExeption ex)
            {
                Console.WriteLine("TestSubExeption");
            }
            catch (TestExeption ex)
            {
                Console.WriteLine("TestExeption");
            }
        }

    }
}
