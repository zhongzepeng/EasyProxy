using System.Reflection;

namespace EasyProxy.HttpServer.Controller
{
    public class ActionExecuteContext
    {
        public HttpRequest HttpRequest { get; set; }

        public HttpResponse HttpResponse { get; set; }

        public IController Controller { get; internal set; }

        public MethodInfo Action { get; internal set; }
    }
}
