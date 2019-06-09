using System;

namespace EasyProxy.HttpServer.Route
{
    public class HttpPostAttribute : Attribute
    {
        public HttpPostAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; set; }
    }
}
