using EasyProxy.HttpServer.Controller;
using System.Reflection;

namespace EasyProxy.HttpServer.Route
{
    public interface IRoute
    {
        (IController, MethodInfo, object) Route(HttpRequest request);
    }
}
