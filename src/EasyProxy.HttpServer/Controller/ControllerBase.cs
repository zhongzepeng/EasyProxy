using EasyProxy.HttpServer.Cookie;
using EasyProxy.HttpServer.Result;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Controller
{
    public abstract class ControllerBase : IController
    {
        public ControllerBase()
        {
            ResponseCookie = new List<HttpCookie>();
        }
        public dynamic TempData => new ExpandoObject();

        public HttpRequest Request { get; internal set; }

        public List<HttpCookie> ResponseCookie { get; }

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

        protected IActionResult View(string viewName, object data = null)
        {

            var clsNmae = GetType().Name;
            var controllerName = clsNmae.Substring(0, clsNmae.Length - "Controller".Length);

            return new ViewResult { ViewName = $"{controllerName}/{viewName}.html", ViewData = data };
        }
    }
}
