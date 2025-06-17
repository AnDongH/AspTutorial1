using System.Net.Http;
using System.Threading.Tasks;
using AspTutorial1.Controllers;

namespace AspTutorial1.Services;

public class TestHttpService
{

    private readonly HttpClient _httpClient;
    
    public TestHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // 여기서 추가 설정
    }
    
    public async Task<string> GetAsync(string url)
    {
        return await _httpClient.GetStringAsync(url);
    }
    
}