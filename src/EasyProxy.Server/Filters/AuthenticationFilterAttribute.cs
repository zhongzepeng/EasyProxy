using EasyProxy.Core;
using EasyProxy.HttpServer.Attributes;
using EasyProxy.HttpServer.Authorization;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Filter;
using EasyProxy.HttpServer.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EasyProxy.Server.Filters
{
    public class AuthenticationFilterAttribute : BaseFilterAttribute
    {
        private readonly ServerOptions options;
        private readonly ILogger logger;

        public AuthenticationFilterAttribute()
        {
            options = ServiceLocator.GetService<IOptions<ServerOptions>>()?.Value;
            logger = ServiceLocator.GetService<ILogger<AuthenticationFilterAttribute>>();
        }
        public override async Task OnActionExecutingAsync(ActionExecuteContext context)
        {
            if (context.Action.IsDefined(typeof(AllowAnonymouseAttribute), false))
            {
                await Task.CompletedTask;
                return;
            }
            var failResut = new HttpStatusCodeResult { StatusCode = 403 };
            if (!context.HttpRequest.Headers.ContainsKey("Authorization"))
            {
                context.HttpResponse = failResut.ExecuteResult();
                return;
            }
            var token = context.HttpRequest.Headers["Authorization"];

            var (success, dic) = JwtHelper.ValidateToken(token, options.Secret);

            if (!success)
            {
                context.HttpResponse = failResut.ExecuteResult();
                return;
            }

            context.Controller.TempData.UserName = dic["username"];
        }
    }
}

