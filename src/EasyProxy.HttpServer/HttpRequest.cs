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

        public Stream Body { get; set; }

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
    }
}
