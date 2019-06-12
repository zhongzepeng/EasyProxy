using EasyProxy.HttpServer.Attributes;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyProxy.Server.Controllers
{
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
            return View("index", new { name = "zzp" });
        }

        [HttpGet("/login")]
        [AllowAnonymouse]
        public IActionResult Login()
        {
            return View("login");
        }

        [HttpGet("/list")]
        public IActionResult List()
        {
            return View("list");
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
