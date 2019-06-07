using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public interface IHttpHandler
    {
        Task<HttpResponse> ProcessAsync(HttpRequest httpRequest);
    }
}
