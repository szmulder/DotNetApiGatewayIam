using System.Threading.Tasks;

namespace TestConsoleApp
{
    public interface IApiTest
    {
        Task<bool> Run();
    }
}