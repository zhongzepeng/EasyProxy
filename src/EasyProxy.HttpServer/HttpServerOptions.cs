using Microsoft.Extensions.Options;

namespace EasyProxy.HttpServer
{
    public class HttpServerOptions : IOptions<HttpServerOptions>
    {
        public string Address { get; set; }

        public int Port { get; set; }

        public HttpServerOptions Value => this;
    }
}
