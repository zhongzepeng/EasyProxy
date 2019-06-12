using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public class HttpRequest
    {
        public HttpMethod HttpMethod { get; private set; }

        public string Version { get; private set; }

        public string Url { get; private set; }

        public IDictionary<string, string> Headers { get; set; }

        private IDictionary<string, string> cookies;
        public IDictionary<string, string> Cookies
        {
            get
            {
                if (cookies == null)
                {
                    cookies = ParseCookies(Headers["Cookie"] ?? string.Empty);
                }
                return cookies;
            }
        }

        private IDictionary<string, string> queryDic;
        public IDictionary<string, string> Query
        {
            get
            {
                if (queryDic != null)
                    return queryDic;
                queryDic = GetQueryDictionary();
                return queryDic;
            }
        }

        public Stream Body { get; set; }

        public string Host
        {
            get
            {
                return Headers.ContainsKey("Host") ? Headers["Host"] : string.Empty;
            }
        }

        public Uri Uri
        {
            get
            {
                return new Uri($"{Host}/{Url}");
            }
        }

        public string QueryString
        {
            get
            {
                return Uri.Query;
            }
        }

        public string AbsolutePath
        {
            get
            {
                return Uri.AbsolutePath;
            }
        }

        internal void ParseStatusLine(string line)
        {
            var subs = line.Split(' ');

            if (subs.Length != 3)
            {
                throw new Exception("invalid http request");
            }
            HttpMethod = Enum.Parse<HttpMethod>(subs[0], true);

            Url = subs[1];

            Version = subs[2];
        }

        internal async Task WriteBodyAsync(byte[] bytes)
        {
            Body = new MemoryStream();
            await Body.WriteAsync(bytes);
            Body.Seek(0, SeekOrigin.Begin);
        }

        internal void ParseHeader(List<string> lines)
        {
            Headers = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                Headers.Add(ParseHeaderLine(line));
            }
        }

        private KeyValuePair<string, string> ParseHeaderLine(string line)
        {
            var index = line.IndexOf(':');
            if (index == -1)
            {
                throw new Exception("invalid http header");
            }
            var key = line.Substring(0, index);
            var value = line.Substring(index + 1).TrimStart();

            return new KeyValuePair<string, string>(key, value);
        }

        private IDictionary<string, string> GetQueryDictionary()
        {
            var dic = new Dictionary<string, string>();
            var query = QueryString;
            if (string.IsNullOrWhiteSpace(query))
            {
                return dic;
            }
            if (query.StartsWith("?"))
            {
                query = query.Substring(1);
            }
            var ps = query.Split('&');
            foreach (var p in ps)
            {
                var parts = p.Split('=');
                if (dic.ContainsKey(parts[0]))
                    continue;
                dic.Add(parts[0].ToLower(), parts[1]);
            }
            return dic;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("HttpRequest:");
            sb.AppendLine($"    HttpMethod:{HttpMethod}");
            sb.AppendLine($"    Version:{Version}");
            sb.AppendLine($"    Url:{Url}");
            sb.AppendLine($"    Headers:{Headers.Count}");
            foreach (var header in Headers)
            {
                sb.AppendLine($"        {header.Key}---{header.Value}");
            }
            if (Body != null)
            {
                sb.AppendLine($"    Body:{Body.Length}");
            }
            return sb.ToString();
        }

        private IDictionary<string, string> ParseCookies(string cookieString)
        {
            var dic = new Dictionary<string, string>();
            var cookieParts = cookieString.Split(';');
            foreach (var part in cookieParts)
            {
                var kv = part.Split('=');
                if (!dic.ContainsKey(kv[0]))
                {
                    dic.Add(kv[0], kv[1]);
                }
            }
            return dic;
        }
    }
}
