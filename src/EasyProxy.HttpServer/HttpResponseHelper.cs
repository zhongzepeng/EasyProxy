using System.IO;
using System.Reflection;
using System.Text;

namespace EasyProxy.HttpServer
{
    public static class HttpResponseHelper
    {
        private static Stream errorPageStream;
        static HttpResponseHelper()
        {
            errorPageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyProxy.HttpServer.DefaultPages.error.html");
        }

        public static HttpResponse CreateDefaultErrorResponse()
        {
            var res = new HttpResponse
            {
                StatusCode = 500
            };
            errorPageStream.Seek(0, SeekOrigin.Begin);
            errorPageStream.CopyTo(res.Body);
            res.Body.Seek(0, SeekOrigin.Begin);
            return res;
        }

        public static byte[] ToHttpProtocolData(this HttpResponse httpResponse)
        {
            httpResponse.Headers.Add("Content-Type", httpResponse.ContentType);
            httpResponse.Headers.Add("Content-Length", httpResponse.Body.Length.ToString());
            var sb = new StringBuilder();
            var statusLine = $"{httpResponse.Version} {httpResponse.StatusCode} {Constants.ReasonPhrase[httpResponse.StatusCode]}";
            sb.AppendLine(statusLine);

            foreach (var header in httpResponse.Headers)
            {
                sb.AppendLine($"{header.Key}:{header.Value}");
            }

            sb.AppendLine("");//blank line
            var protocolData = sb.ToString();
            var stream = new MemoryStream();
            stream.Write(Constants.DefaultEncoding.GetBytes(protocolData));
            httpResponse.Body.CopyTo(stream);
            return stream.GetBuffer();
        }
    }
}
