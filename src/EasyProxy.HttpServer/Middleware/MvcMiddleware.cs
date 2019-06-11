﻿using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Filter;
using EasyProxy.HttpServer.Result;
using EasyProxy.HttpServer.Route;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

                var filterList = GetFilters(controller, methodInfo);
                var stack = new Stack<IFilter>();
                for (var i = 0; i < filterList.Count; i++)
                {
                    var filter = filterList[i];
                    await filter.OnActionExecutingAsync(context);
                    if (context.Final)
                    {
                        return context.HttpResponse;
                    }
                    stack.Push(filter);
                }

                await controller.OnActionExecutingAsync(context);

                if (context.Final)
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

                context.HttpResponse = actionResult.ExecuteResult();

                await controller.OnActionExecutedAsync(context);

                if (context.Final)
                {
                    return context.HttpResponse;
                }

                while (stack.Count != 0)
                {
                    var filter = stack.Pop();
                    await filter.OnActionExecutedAsync(context);

                    if (context.Final)
                    {
                        return context.HttpResponse;
                    }
                }
                return context.HttpResponse;
            }
            catch (System.Exception e)
            {
                return HttpResponseHelper.CreateDefaultErrorResponse(e);
            }
        }

        private List<IFilter> GetFilters(IController controller, MethodInfo action)
        {
            var filters = action.GetCustomAttributes()
                 .Where(x => typeof(IFilter).IsAssignableFrom(x.GetType()) && !x.GetType().IsAbstract)
                 .Select(x => x as IFilter).ToList();
            var controlerFilters = controller.GetType().GetCustomAttributes()
                .Where(x => typeof(IFilter).IsAssignableFrom(x.GetType()) && !x.GetType().IsAbstract)
                 .Select(x => x as IFilter).ToList();
            filters = filters.Union(controlerFilters).ToList();
            filters.Sort(new FilterComparer());
            return filters;
        }
    }

    internal class FilterComparer : IComparer<IFilter>
    {
        public int Compare(IFilter x, IFilter y)
        {
            return x.Order.CompareTo(y.Order);
        }
    }
}