using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Controller
{
    public interface IController
    {
        Task OnActionExecutingAsync(ActionExecuteContext context);
        Task ExecuteAsync(ActionExecuteContext context);
        Task OnActionExecutedAsync(ActionExecuteContext context);
    }
}
