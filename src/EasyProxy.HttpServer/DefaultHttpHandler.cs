using EasyProxy.HttpServer.Middleware;
using EasyProxy.HttpServer.Route;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    internal class DefaultHttpHandler : IHttpHandler
    {
        private readonly IRoute route;

        public DefaultHttpHandler(IRoute route)
        {
            this.route = route;
        }

        public async Task<HttpResponse> ProcessAsync(HttpRequest httpRequest)
        {
            var chain = BuildMiddlewareChain();
            return await chain.Invoke(httpRequest);
        }

        private IMiddleware BuildMiddlewareChain()
        {
            var builder = new MiddlewareChainBuilder();
            builder.Use(new StaticFileMiddleware());
            builder.Use(new MvcMiddleware(route));
            return builder.Build();
        }
    }
}
