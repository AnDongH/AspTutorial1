using System.Text.RegularExpressions;
using AspTutorial1.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace AspTutorial1;

// IValidateOptions 구현 -> 유효성 검사 코드를 Program.cs에서 클래스로 이동시킬 수 있음
public class MyConfigValidation : IValidateOptions<MyConfigOptions2>
{
    public MyConfigOptions2 _config { get; private set; }

    public  MyConfigValidation(IConfiguration config)
    {
        _config = config.GetSection(MyConfigOptions2.MyConfig)
            .Get<MyConfigOptions2>();
    }

    public ValidateOptionsResult Validate(string name, MyConfigOptions2 options)
    {
        string? vor = null;
        var rx = new Regex(@"^[a-zA-Z''-'\s]{1,40}$");
        var match = rx.Match(options.Key1!);

        if (string.IsNullOrEmpty(match.Value))
        {
            vor = $"{options.Key1} doesn't match RegEx \n";
        }

        if ( options.Key2 < 0 || options.Key2 > 1000)
        {
            vor = $"{options.Key2} doesn't match Range 0 - 1000 \n";
        }

        if (_config.Key2 != default)
        {
            if(_config.Key3 <= _config.Key2)
            {
                vor +=  "Key3 must be > than Key2.";
            }
        }

        if (vor != null)
        {
            return ValidateOptionsResult.Fail(vor);
        }

        return ValidateOptionsResult.Success;
    }
}