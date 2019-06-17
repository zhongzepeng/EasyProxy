using DotLiquid;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Result
{
    internal class DefaultPageResult : IActionResult
    {
        private static readonly Dictionary<int, Template> viewCache = new Dictionary<int, Template>();

        private static readonly Dictionary<int, string> fileNameDic = new Dictionary<int, string>
        {
            {500,"error" },
            {404,"notfound" }
        };

        public int StatusCode { get; set; }

        public object ViewData { get; set; }

        public async Task<HttpResponse> ExecuteResultAsync()
        {
            Template template;
            if (viewCache.ContainsKey(StatusCode))
            {
                template = viewCache[StatusCode];
            }
            else
            {
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream($"EasyProxy.HttpServer.DefaultPages.{fileNameDic[StatusCode]}.html"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        template = Template.Parse(await reader.ReadToEndAsync());
                        viewCache.Add(StatusCode, template);
                    }
                }
            }
            var content = template.Render(Hash.FromAnonymousObject(ViewData));

            var res = new HttpResponse
            {
                StatusCode = StatusCode
            };

            await res.WriteBodyAsync(Constants.DefaultEncoding.GetBytes(content));
            return res;
        }
    }
}
