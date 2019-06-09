using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace TestConsoleApp
{
    public class AppStart
    {
        private readonly IApiTest _apiTest;
        private readonly AppSettings _config;

        public AppStart(IApiTest apiTest, IOptions<AppSettings> config)
        {
            _apiTest = apiTest;
            _config = config.Value;
        }

        public async Task<bool> Run()
        {
            var result = await _apiTest.Run();

            return result;
        }

    }
}
