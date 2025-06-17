using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspTutorial1;

public class ValidateHeaderHandler : DelegatingHandler
{
    // 여기서 종속성 주입으로 서비스도 가져올 수 있음!
    public ValidateHeaderHandler()
    {
        
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("Test-Header"))
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Test-Header is required.")
            };
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

