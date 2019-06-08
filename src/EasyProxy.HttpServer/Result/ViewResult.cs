using System.Collections.Generic;
using System.Reflection;

namespace EasyProxy.HttpServer.Result
{
    /// <summary>
    /// 使用html作为模板，简单使用，不支持传数据到模板中
    /// </summary>
    public class ViewResult : IActionResult
    {
        private readonly Dictionary<string, byte[]> viewCache = new Dictionary<string, byte[]>();

        private static string baseViewPath = string.Empty;

        static ViewResult()
        {
            baseViewPath = Assembly.GetEntryAssembly().GetName().Name;
        }

        public string ViewName { get; set; }

        public HttpResponse ExecuteResult()
        {
            var absoluteName = $"{baseViewPath}.Views.{ViewName}";
            byte[] content;
            if (viewCache.ContainsKey(absoluteName))
            {
                content = viewCache[absoluteName];
            }
            else
            {
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(absoluteName))
                {
                    content = new byte[stream.Length];
                    stream.Read(content);
                    viewCache.Add(absoluteName, content);
                }
            }
            var res = new HttpResponse();

            res.WriteBody(content);
            return res;
        }
    }
}
