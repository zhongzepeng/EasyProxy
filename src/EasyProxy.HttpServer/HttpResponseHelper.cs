using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EasyProxy.HttpServer
{
    public static class HttpResponseHelper
    {
        private static Stream errorPageStream;
        private static Stream deafultPageStream;
        private static Stream notfoundPageStream;
        static HttpResponseHelper()
        {
            notfoundPageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyProxy.HttpServer.DefaultPages.notfound.html");
            deafultPageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyProxy.HttpServer.DefaultPages.default.html");
            errorPageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyProxy.HttpServer.DefaultPages.error.html");
        }

        public static HttpResponse CreateDefaultPageResponse()
        {
            var res = new HttpResponse();
            deafultPageStream.Seek(0, SeekOrigin.Begin);
            deafultPageStream.CopyTo(res.Body);
            res.Body.Seek(0, SeekOrigin.Begin);
            return res;
        }

        public static HttpResponse CreateDefaultErrorResponse(Exception e)
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

        public static HttpResponse CreateNotFoundResponse()
        {
            var res = new HttpResponse
            {
                StatusCode = 404
            };
            res.WriteBody(notfoundPageStream);
            return res;
        }


        public static byte[] ToHttpProtocolData(this HttpResponse httpResponse)
        {
            SetCookies(httpResponse);
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

        private static void SetCookies(HttpResponse httpResponse)
        {
            if (!httpResponse.Cookies.Any())
            {
                return;
            }
            foreach (var cookie in httpResponse.Cookies)
            {
                httpResponse.Headers.Add("Set-Cookie", cookie.GetHeaderString());
            }
        }
    }
}
