using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Result
{
    public interface IActionResult
    {
        Task<HttpResponse> ExecuteResultAsync();
    }
}
