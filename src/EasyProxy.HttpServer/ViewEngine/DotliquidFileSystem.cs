using DotLiquid;
using DotLiquid.FileSystems;
using System.IO;

namespace EasyProxy.HttpServer.ViewEngine
{
    public class DotliquidFileSystem : IFileSystem
    {
        public string ReadTemplateFile(Context context, string templateName)
        {
            var path = $"{Directory.GetCurrentDirectory()}/{templateName}";
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
