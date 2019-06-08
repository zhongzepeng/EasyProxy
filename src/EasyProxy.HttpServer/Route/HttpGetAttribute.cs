using System;

namespace EasyProxy.HttpServer.Route
{
    public class HttpGetAttribute : Attribute
    {
        public HttpGetAttribute(string path)
        {
            Path = path;
        }
        public string Path { get; set; }
    }
}
