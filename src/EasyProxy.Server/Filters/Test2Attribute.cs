using EasyProxy.Core;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Filter;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EasyProxy.Server.Filters
{
    public class Test2Attribute : BaseFilterAttribute
    {
        private readonly ILogger logger;
        public Test2Attribute()
        {
            logger = ServiceLocator.GetService<ILogger<Test1Attribute>>();
        }

        public override Task OnActionExecutingAsync(ActionExecuteContext context)
        {
            logger.LogInformation("test2 ing");
            return Task.CompletedTask;
        }

        public override Task OnActionExecutedAsync(ActionExecuteContext context)
        {
            logger.LogInformation("test2 ed");
            return Task.CompletedTask;
        }
    }
}
