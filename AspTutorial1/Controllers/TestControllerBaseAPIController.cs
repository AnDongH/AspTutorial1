using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestControllerBaseAPIController : ControllerBase
    {
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<TestItemBody> Create(string name)
        {

            TestItemBody itemBody = new TestItemBody() { Name = name };
            itemBody.Id = Random.Shared.Next();
            
            return CreatedAtAction("TestAction", new { id = itemBody.Id }, itemBody);
        }

        [HttpGet("{id}")]
        public IActionResult TestAction(int id)
        {
            return Content(id.ToString());
        }

        [HttpPost("[action]")]
        public IActionResult TestModelBinding([FromHeader] int token, [FromBody] TestItemBody itemBody)
        {
            Console.WriteLine(token);
            if (token < 0)
            {
                return BadRequest();
            }
            
            return Content(itemBody.Name);
        }
        
        // 기본적으로 [FromBody] 안하면 [FromQuery]로 인식한다.
        [HttpPost("[action]")]
        public IActionResult TestModelBindingGuess([FromBody] string data)
        {
            return Content(data);
        }

        // 가장 기본 반환
        [HttpGet("[action]")]
        public List<Product> GetNormal()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "Normal1", Price = 100 },
                new Product() { Id = 2, Name = "Normal2", Price = 200 },
                new Product() { Id = 3, Name = "Normal3", Price = 300 },
            };

            return res;
        }
        
        // Task 반환
        [HttpGet("[action]")]
        public Task<List<Product>> GetTask()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "Task1", Price = 100 },
                new Product() { Id = 2, Name = "Task2", Price = 200 },
                new Product() { Id = 3, Name = "Task3", Price = 300 },
            };

            return Task.FromResult(res);
            
        }
        
        [HttpGet("[action]")]
        public async Task<List<Product>> GetAsyncTask()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "Task1", Price = 100 },
                new Product() { Id = 2, Name = "Task2", Price = 200 },
                new Product() { Id = 3, Name = "Task3", Price = 300 },
            };

            await Task.Delay(1000);
            
            return res;

        }

        // IEnumerable 반환
        [HttpGet("[action]")]
        public IEnumerable<Product> GetIEnumerable()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "IEnumerable1", Price = 100 },
                new Product() { Id = 2, Name = "IEnumerable2", Price = 200 },
                new Product() { Id = 3, Name = "IEnumerable3", Price = 300 },
            };

            foreach (var p in res)
            {
                yield return p;
            }
        }
        
        // IAsyncEnumerable 반환
        [HttpGet("[action]")]
        public async IAsyncEnumerable<Product> GetIAsyncEnumerable()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "IEnumerable1", Price = 100 },
                new Product() { Id = 2, Name = "IEnumerable2", Price = 200 },
                new Product() { Id = 3, Name = "IEnumerable3", Price = 300 },
            }.AsIAsyncEnumerable();

            await foreach (var p in res)
            {
                yield return p;
            }
        }

        [HttpGet("[action]")]
        [ProducesResponseType<Product>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] // 이거 추가한다고 이것만 할 수 있는건 아니고 도큐먼트에 이런식으로 기제되는듯
        public IActionResult GetIActionResult()
        {
            
            var res = new List<Product>()
            {
                new Product() { Id = 1, Name = "IEnumerable1", Price = 100 },
                new Product() { Id = 2, Name = "IEnumerable2", Price = 200 },
                new Product() { Id = 3, Name = "IEnumerable3", Price = 300 },
            };

            res = null;

            return res != null ? Ok(res) : NotFound();

        }
        
        [HttpPost("[action]")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateAsync_IActionResult(Product product)
        {
            if (product.Id < 0)
            {
                return BadRequest();
            }

            // 주로 Post작업에서 사용하는 메서드. 201상태코드와 함께 생성된 리소스의 URI와 리소스 정보를 리턴
            return CreatedAtAction("CreateAsync_IActionResult", new { id = product.Id }, product);
        }

        [HttpGet("[action]")]
        public ActionResult<Product> GetActionResultT()
        {
            var res = new Product() { Id = 1, Name = "ActionResult1", Price = 100 };
            return res;
        }
        
    }

    public class Product
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    
    public static class ProductRepository
    {
        // 비동기 스트림을 반환하는 메서드
        public static async IAsyncEnumerable<Product> AsIAsyncEnumerable(this IEnumerable<Product> products)
        {
            // 데이터베이스나 API에서 제품을 가져오는 것을 시뮬레이션
            foreach (var product in products)
            {
                // 네트워크 지연이나 I/O 작업을 시뮬레이션
                await Task.Delay(200);
                yield return product;
            }
        }
    }

    public class TestItemHeader
    {
        public int Token { get; set; }
    }
    
    public class TestItemBody
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    
}
