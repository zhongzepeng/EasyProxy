using System;

namespace EasyProxy.HttpServer.Cookie
{
    public class HttpCookie
    {
        public string Domain { get; set; }

        public string Path { get; set; } = "/";

        public DateTime? Expires { get; set; }

        public int? MaxAge { get; set; }

        public bool HttpOnly { get; set; }

        public bool Secure { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        internal string GetHeaderString()
        {
            var headerstring = $"{Name}={Value};Path={Path}";
            if (Expires.HasValue)
            {
                headerstring = $"{headerstring}Expires={Expires.Value}";
            }
            else if (MaxAge.HasValue)
            {
                headerstring = $"{headerstring};MaxAge={MaxAge.Value}";
            }

            if (!string.IsNullOrEmpty(Domain))
            {
                headerstring = $"{headerstring};Domain={Domain}";
            }

            if (Secure)
            {
                headerstring = $"{headerstring};Secure";
            }
            if (HttpOnly)
            {
                headerstring = $"{headerstring};HttpOnly";
            }
            return headerstring;
        }
    }
}
