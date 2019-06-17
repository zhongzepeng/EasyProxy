using EasyProxy.HttpServer.Result;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public static class HttpResponseHelper
    {

        public static async Task<HttpResponse> CreateDefaultErrorResponseAsync(Exception e)
        {
            var defaultPageResult = new DefaultPageResult
            {
                StatusCode = 500,
            };

            return await defaultPageResult.ExecuteResultAsync();
        }

        public static async Task<HttpResponse> CreateNotFoundResponseAsync()
        {
            var defaultPageResult = new DefaultPageResult
            {
                StatusCode = 404,
            };

            return await defaultPageResult.ExecuteResultAsync();
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
