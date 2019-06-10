using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Middleware
{
    public abstract class MiddlewareBase : IMiddleware
    {
        public IMiddleware Next { get; set; }

        public abstract Task<HttpResponse> Invoke(HttpRequest request);
    }
}
