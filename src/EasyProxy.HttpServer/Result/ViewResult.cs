using DotLiquid;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EasyProxy.HttpServer.Result
{
    /// <summary>
    /// 使用html作为模板，简单使用，不支持传数据到模板中
    /// </summary>
    public class ViewResult : IActionResult
    {
        private readonly Dictionary<string, Template> viewCache = new Dictionary<string, Template>();

        private static string baseViewPath = string.Empty;

        static ViewResult()
        {
            baseViewPath = Assembly.GetEntryAssembly().GetName().Name;
        }

        public string ViewName { get; set; }

        public object ViewData { get; set; }

        public HttpResponse ExecuteResult()
        {
            var absoluteName = $"Views/{ViewName}";
            Template template;
            if (viewCache.ContainsKey(absoluteName))
            {
                template = viewCache[absoluteName];
            }
            else
            {
                var templateStr = ReadTemplateFile(absoluteName);
                template = Template.Parse(templateStr);
                viewCache.Add(absoluteName, template);
            }
            var content = template.Render(Hash.FromAnonymousObject(ViewData));

            var res = new HttpResponse();

            res.WriteBody(Constants.DefaultEncoding.GetBytes(content));
            return res;
        }

        private string ReadTemplateFile(string absoluteName)
        {
            var path = $"{Directory.GetCurrentDirectory()}/{absoluteName}";
            if (!File.Exists(path))
            {
                return string.Empty;
            }
            using (var reader = File.OpenText(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
