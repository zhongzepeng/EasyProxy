using EasyProxy.HttpServer.Cookie;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public class HttpResponse
    {
        public HttpResponse()
        {
            Body = new MemoryStream();
            Headers = new Dictionary<string, string>();
            Cookies = new List<HttpCookie>();
        }
        public int StatusCode { get; set; } = 200;

        public string Version { get; set; } = "HTTP/1.1";

        public IDictionary<string, string> Headers { get; set; }

        public Stream Body { get; set; }

        public List<HttpCookie> Cookies { get; set; }

        public string ContentType { get; set; } = "text/html; charset=utf-8";

        public async Task WriteBodyAsync(byte[] content)
        {
            await Body.WriteAsync(content);
            Body.Seek(0, SeekOrigin.Begin);
        }

        public async Task WriteBodyAsync(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            await stream.CopyToAsync(Body);
            Body.Seek(0, SeekOrigin.Begin);
        }
    }
}
