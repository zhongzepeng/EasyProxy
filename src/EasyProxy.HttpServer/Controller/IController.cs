using EasyProxy.HttpServer.Cookie;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Controller
{
    public interface IController
    {
        HttpRequest Request { get; }

        List<HttpCookie> ResponseCookie { get; }
        dynamic TempData { get; }
        Task OnActionExecutingAsync(ActionExecuteContext context);
        Task OnActionExecutedAsync(ActionExecuteContext context);
    }
}
