using System;

namespace EasyProxy.HttpServer.Route
{
    public class PrefixAttribute : Attribute
    {
        public PrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
        public string Prefix { get; set; }
    }
}
