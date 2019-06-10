using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Middleware
{
    public interface IMiddleware
    {
        IMiddleware Next { get; set; }
        Task<HttpResponse> Invoke(HttpRequest request);
    }
}
