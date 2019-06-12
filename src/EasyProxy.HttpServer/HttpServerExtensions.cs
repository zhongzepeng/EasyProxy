using DotLiquid;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Filter;
using EasyProxy.HttpServer.Route;
using EasyProxy.HttpServer.ViewEngine;
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
            Template.FileSystem = new DotliquidFileSystem();
            //RegisterFilterTypes(services);
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

        private static void RegisterFilterTypes(IServiceCollection services)
        {
            var filters = Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(IFilter).IsAssignableFrom(type));

            foreach (var filter in filters)
            {
                services.AddTransient(filter);
            }
        }
    }
}
