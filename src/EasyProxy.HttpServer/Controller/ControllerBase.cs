using EasyProxy.HttpServer.Result;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Controller
{
    public abstract class ControllerBase : IController
    {
        public dynamic TempData => new { };

        public async virtual Task OnActionExecutedAsync(ActionExecuteContext context)
        {
            await Task.CompletedTask;
        }

        public async virtual Task OnActionExecutingAsync(ActionExecuteContext context)
        {
            await Task.CompletedTask;
        }

        protected IActionResult Json(object data)
        {
            return new JsonResult(data);
        }

        protected IActionResult View(string viewName)
        {

            var clsNmae = GetType().Name;
            var controllerName = clsNmae.Substring(0, clsNmae.Length - "Controller".Length);

            return new ViewResult { ViewName = $"{controllerName}.{viewName}.html" };
        }
    }
}
