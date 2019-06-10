using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Middleware
{
    public class MvcMiddleware : MiddlewareBase
    {
        private readonly IRoute route;
        public MvcMiddleware(IRoute route)
        {
            this.route = route;
        }

        public override async Task<HttpResponse> Invoke(HttpRequest httpRequest)
        {
            try
            {
                var context = new ActionExecuteContext
                {
                    HttpRequest = httpRequest
                };

                var (controller, methodInfo, parameter) = route.Route(httpRequest);
                if (controller == null)
                {
                    return HttpResponseHelper.CreateNotFoundResponse();
                }

                context.Controller = controller;
                context.Action = methodInfo;

                await controller.OnActionExecutedAsync(context);

                if (context.HttpResponse != null)
                {
                    return context.HttpResponse;
                }
                IActionResult actionResult;
                if (parameter == null)
                {
                    actionResult = methodInfo.Invoke(controller, new object[] { }) as IActionResult;
                }
                else
                {
                    actionResult = methodInfo.Invoke(controller, new object[] { parameter }) as IActionResult;
                }

                return actionResult.ExecuteResult();
            }
            catch (System.Exception e)
            {
                return HttpResponseHelper.CreateDefaultErrorResponse(e);
            }
        }
    }
}
