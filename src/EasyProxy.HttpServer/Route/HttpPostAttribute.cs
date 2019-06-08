using System;

namespace EasyProxy.HttpServer.Route
{
    public class HttpPostAttribute : Attribute
    {
        public string Path { get; set; }
    }
}
