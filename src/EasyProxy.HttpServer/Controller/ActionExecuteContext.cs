using EasyProxy.HttpServer.Attributes;
using System.Reflection;

namespace EasyProxy.HttpServer.Controller
{
    public class ActionExecuteContext
    {
        public HttpRequest HttpRequest { get; set; }

        public HttpResponse HttpResponse { get; set; }

        public IController Controller { get; internal set; }

        public MethodInfo Action { get; internal set; }

        public bool Final { get; set; }

        public bool IsApiController
        {
            get
            {
                return Controller.GetType().IsDefined(typeof(ApiControllerAttribute));
            }
        }
    }
}
