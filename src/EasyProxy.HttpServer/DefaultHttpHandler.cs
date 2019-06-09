using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    internal class DefaultHttpHandler : IHttpHandler
    {
        private readonly IRoute route;

        public DefaultHttpHandler(IRoute route)
        {
            this.route = route;
        }

        public async Task<HttpResponse> ProcessAsync(HttpRequest httpRequest)
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
