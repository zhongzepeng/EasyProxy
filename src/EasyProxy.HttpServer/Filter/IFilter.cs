using EasyProxy.HttpServer.Controller;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Filter
{
    public interface IFilter
    {
        int Order { get; set; }
        Task OnActionExecutingAsync(ActionExecuteContext context);
        Task OnActionExecutedAsync(ActionExecuteContext context);
    }
}
