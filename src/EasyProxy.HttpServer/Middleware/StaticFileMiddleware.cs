using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Middleware
{
    public class StaticFileMiddleware : MiddlewareBase
    {
        const string DefaultCacheStrategy = "private";
        const string DefaultRoot = "Resource";
        public Dictionary<string, string> EnableFileTypes { get; private set; } = new Dictionary<string, string>
        {
            {"js"  ,"text/javascript"}  ,
            {"png" ,"image/png"}  ,
            {"xml" ,"text/xml"}  ,
            {"jpg" ,"image/jpg"}  ,
            {"bmp" ,"image/bmp"}  ,
            {"jpeg","image/jpeg"}  ,
            {"ico" ,"image/x-ico"}  ,
            {"css" , "text/css"}
        };

        private readonly IFileProvider fileProvider;

        public StaticFileMiddleware()
        {
            fileProvider = new PhysicalFileProvider($"{Directory.GetCurrentDirectory()}/{DefaultRoot}");
        }

        public override async Task<HttpResponse> Invoke(HttpRequest request)
        {
            var ext = GetExtension(request.Url);

            if (ext == string.Empty)
            {
                return await Next?.Invoke(request);
            }

            if (!EnableFileTypes.Keys.Any(x => x.ToLower().Equals(ext)))
            {
                return await Next?.Invoke(request);
            }

            var fileinfo = fileProvider.GetFileInfo(request.Url);

            if (!fileinfo.Exists)
            {
                return HttpResponseHelper.CreateNotFoundResponse();
            }

            using (var stream = fileinfo.CreateReadStream())
            {
                var res = new HttpResponse
                {
                    ContentType = EnableFileTypes[ext],
                };
                res.WriteBody(stream);
                res.Headers.Add("Cache-Control", DefaultCacheStrategy);
                return res;
            }
        }


        private string GetExtension(string url)
        {
            var index = url.LastIndexOf('.');
            if (index > 0)
            {
                return url.Substring(index + 1, url.Length - index - 1);
            }
            return string.Empty;
        }
    }
}
