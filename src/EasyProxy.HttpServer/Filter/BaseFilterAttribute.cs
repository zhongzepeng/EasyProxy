using EasyProxy.HttpServer.Controller;
using System;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Filter
{
    public abstract class BaseFilterAttribute : Attribute, IFilter
    {
        public int Order { get; set; }

        public virtual Task OnActionExecutedAsync(ActionExecuteContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnActionExecutingAsync(ActionExecuteContext context)
        {
            return Task.CompletedTask;
        }
    }
}
