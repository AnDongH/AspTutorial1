using MemoryPack;

namespace AspClient;

class Program
{
    
    static HttpClient client = new HttpClient();
    
    static async Task Main(string[] args)
    {
        using var m = await client.GetAsync("http://10.0.2.51/Gate");

        
        
    }
}