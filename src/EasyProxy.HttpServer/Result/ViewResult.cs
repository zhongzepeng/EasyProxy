using DotLiquid;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace EasyProxy.HttpServer.Result
{
    public class ViewResult : IActionResult
    {
        private readonly Dictionary<string, Template> viewCache = new Dictionary<string, Template>();

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
                var templateStr = Template.FileSystem.ReadTemplateFile(new Context(CultureInfo.CurrentCulture), absoluteName);
                template = Template.Parse(templateStr);
                viewCache.Add(absoluteName, template);
            }
            var content = template.Render(Hash.FromAnonymousObject(ViewData));

            var res = new HttpResponse();

            res.WriteBody(Constants.DefaultEncoding.GetBytes(content));
            return res;
        }
    }
}
