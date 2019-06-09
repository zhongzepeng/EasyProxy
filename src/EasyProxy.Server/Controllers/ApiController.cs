using EasyProxy.Core.Config;
using EasyProxy.HttpServer.Attributes;
using EasyProxy.HttpServer.Authorization;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using EasyProxy.Server.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyProxy.Server.Controllers
{
    [Prefix("/api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> logger;
        private readonly ConfigHelper configHelper;
        private readonly ServerOptions options;
        protected string UserName { get; private set; }
        public ApiController(IOptions<ServerOptions> options, ILogger<ApiController> logger, ConfigHelper configHelper)
        {
            this.logger = logger;
            this.configHelper = configHelper;
            this.options = options.Value;
        }

        [AllowAnonymouse]
        [HttpPost("/login")]
        public IActionResult Login(LoginInputDto input)
        {
            if (!CheckPwd(input))
            {
                return Json(new ResultDto() { Message = "invalid username or password" });
            }
            var token = JwtHelper.GenerateJwtToken(options.Secret, new Dictionary<string, string>
            {
                { "username",input.UserName},
                { "logintime",DateTime.Now.ToShortDateString()}
            });
            return Json(new ResultDto<string>(token) { Success = true });
        }

        private bool CheckPwd(LoginInputDto input)
        {
            return input.UserName == options.UserName && input.Password == options.Password;
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

            UserName = dic["username"];
        }
    }
}
