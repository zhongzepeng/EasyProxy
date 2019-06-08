using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;

namespace EasyProxy.Server.Controllers
{
    [Prefix("/dashboard")]
    public class DashboardController : ControllerBase
    {
        [HttpGet("/index")]
        public IActionResult Index(Param param)
        {
            return View("index");
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View("login");
        }
    }

    public class Param
    {
        public int PageIndex { get; set; }
    }
}
