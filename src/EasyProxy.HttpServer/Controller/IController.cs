using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Controller
{
    public interface IController
    {
        dynamic TempData { get; }
        Task OnActionExecutingAsync(ActionExecuteContext context);
        Task OnActionExecutedAsync(ActionExecuteContext context);
    }
}
