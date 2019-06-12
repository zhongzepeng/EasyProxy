using EasyProxy.Core.Config;
using EasyProxy.HttpServer.Attributes;
using EasyProxy.HttpServer.Authorization;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Cookie;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using EasyProxy.Server.Dtos;
using EasyProxy.Server.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyProxy.Server.Controllers
{
    [AuthenticationFilter]
    public class AdminController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly ServerOptions options;
        private readonly ConfigHelper configHelper;
        public AdminController(IOptions<ServerOptions> options, ILogger<AdminController> logger, ConfigHelper configHelper)
        {
            this.options = options?.Value;
            this.logger = logger;
            this.configHelper = configHelper;
        }

        [HttpGet("/index")]
        public async Task<IActionResult> IndexAsync()
        {
            var clients = (await configHelper.GetAllClientAsync()).Select(x => new
            {
                ChannelCount = x.Channels.Count,
                x.ClientId,
                x.Name,
                x.SecretKey
            }).ToList();
            return View("index", new { clients });
        }

        [HttpGet("/login")]
        [AllowAnonymouse]
        public IActionResult Login()
        {
            return View("login");
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
                { "logintime",DateTime.Now.ToString()}
            });
            ResponseCookie.Add(new HttpCookie
            {
                Name = "authorization",
                Value = token
            });
            return Json(new ResultDto() { Success = true });
        }
        private bool CheckPwd(LoginInputDto input)
        {
            return input.UserName == options.UserName && input.Password == options.Password;
        }

        [HttpGet("/add")]
        public IActionResult Add()
        {
            return View("add");
        }

        [HttpPost("/add")]
        public async Task<IActionResult> AddAsync(AddChannelInputDto input)
        {

            await configHelper.AddClientAsync(new ClientConfig
            {
                Name = input.Name,
                SecretKey = input.SecretKey
            });
            return Json(new ResultDto { Success = true });
        }

        [HttpPost("/del")]
        public IActionResult Delete()
        {
            return View("add");
        }

        public override Task OnActionExecutedAsync(ActionExecuteContext context)
        {
            logger.LogInformation("controller ed");
            return Task.CompletedTask;
        }

        public override Task OnActionExecutingAsync(ActionExecuteContext context)
        {
            logger.LogInformation("controller ing");
            return Task.CompletedTask;
        }
    }
}
