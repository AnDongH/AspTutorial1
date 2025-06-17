using AspTutorial1.Models.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspTutorial1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestOptionPatternController : ControllerBase
    {
        
        // 구성 가져오는 법
        private readonly IConfiguration Configuration;
        private readonly PositionOptions _options;
        
        private readonly TopItemSettings _monthTopItem;
        private readonly TopItemSettings _yearTopItem;
        
        private readonly ILogger<TestOptionPatternController> _logger;
        private readonly IOptions<MyConfigOptions> _config;
        private readonly IOptions<MyConfigOptions2> _config2;
        
        public TestOptionPatternController(
            IConfiguration configuration, 
            IOptions<PositionOptions> options, 
            IOptionsSnapshot<TopItemSettings> topItemSettings, 
            IOptions<MyConfigOptions> config,
            IOptions<MyConfigOptions2> config2,
            ILogger<TestOptionPatternController> logger)
        {
            Configuration = configuration;
            _options = options.Value;
            _monthTopItem = topItemSettings.Get(TopItemSettings.Month);
            _yearTopItem = topItemSettings.Get(TopItemSettings.Year);
            
            _config = config;
            _config2 = config2;
            _logger = logger;
            
            try
            {
                var configValue = _config.Value;

            }
            catch (OptionsValidationException ex)
            {
                foreach (var failure in ex.Failures)
                {
                    _logger.LogError(failure);
                }
            }
            
        }
        
        [HttpGet("standard/bind")]
        public IActionResult GetStandardBind()
        {
            var positionOptions = new PositionOptions();
            Configuration.GetSection(PositionOptions.Position).Bind(positionOptions);

            return Content($"Title: {positionOptions.Title} \n" +
                           $"Name: {positionOptions.Name}");
        }
        
        [HttpGet("standard/get")]
        public IActionResult GetStandardGet()
        {
            var positionOptions = Configuration.GetSection(PositionOptions.Position)
                .Get<PositionOptions>();

            return Content($"Title: {positionOptions.Title} \n" +
                           $"Name: {positionOptions.Name}");
        }
        
        [HttpGet("standard/abstract")]
        public IActionResult GetStandardAbstract()
        {
            var nameTitleOptions = new NameTitleOptions(22);
            Configuration.GetSection(NameTitleOptions.NameTitle).Bind(nameTitleOptions);

            return Content($"Title: {nameTitleOptions.Title} \n" +
                           $"Name: {nameTitleOptions.Name}  \n" +
                           $"Age: {nameTitleOptions.Age}"
            );
        }
        
        [HttpGet("standard/option-pattern")]
        public IActionResult GetStandardOptionPattern()
        {
            return Content($"Title: {_options.Title} \n" +
                           $"Name: {_options.Name}");
        }
        
        [HttpGet("named/option-pattern")]
        public IActionResult GetNamedOptionPattern()
        {
            return Content($"Month:Name {_monthTopItem.Name} \n" +
                           $"Month:Model {_monthTopItem.Model} \n\n" +
                           $"Year:Name {_yearTopItem.Name} \n" +
                           $"Year:Model {_yearTopItem.Model} \n"   );
        }
        
        [HttpGet("option-validate")]
        public IActionResult GetOptionValidate()
        {
            string msg;
            try
            {
                msg = $"Key1: {_config.Value.Key1} \n" +
                      $"Key2: {_config.Value.Key2} \n" +
                      $"Key3: {_config.Value.Key3}";
            }
            catch (OptionsValidationException optValEx)
            {
                return Content(optValEx.Message);
            }
            return Content(msg);
        }
        
        [HttpGet("option-validate/ivalidateoptions")]
        public IActionResult GetOptionValidateIValidateoptions()
        {
            string msg;
            try
            {
                msg = $"Key1: {_config2.Value.Key1} \n" +
                      $"Key2: {_config2.Value.Key2} \n" +
                      $"Key3: {_config2.Value.Key3}";
            }
            catch (OptionsValidationException optValEx)
            {
                return Content(optValEx.Message);
            }
            return Content(msg);
        }
        
    }
    
}
