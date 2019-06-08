using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Route;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace EasyProxy.HttpServer
{
    public static class HttpServerExtensions
    {
        public static void AddHttpServer(this IServiceCollection services, IConfigurationSection section)
        {
            services.Configure<HttpServerOptions>(section);
            services.AddSingleton(typeof(IRoute), typeof(EasyRoute));
            services.AddSingleton(typeof(IHttpHandler), typeof(DefaultHttpHandler));
            services.AddSingleton(typeof(EasyHttpServer), typeof(EasyHttpServer));
            RegisterControllerTypes(services);
        }

        internal static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        private static void RegisterControllerTypes(IServiceCollection services)
        {
            var controllers = Assembly
                 .GetEntryAssembly()
                 .GetTypes()
                 .Where(type => !type.IsAbstract && typeof(IController).IsAssignableFrom(type));

            foreach (var controller in controllers)
            {
                services.AddTransient(controller);
            }
        }
    }
}
