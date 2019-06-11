using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using EasyProxy.Server.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyProxy.Server.Controllers
{
    [Test2(Order = 1)]
    [Prefix("/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger logger;
        public DashboardController(ILogger<DashboardController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("/index")]
        public IActionResult Index()
        {
            return View("index");
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View("login");
        }

        [Test1(Order = 0)]
        [HttpGet("/test")]
        public IActionResult Test()
        {
            return Json(new { a = 1 });
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
