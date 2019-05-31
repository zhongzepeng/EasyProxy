using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ProxyServer>();
            var configuration = BuildConfiguration();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(configure => configure.AddConsole());
            serviceCollection.Configure<ServerOptions>(configuration.GetSection("ServerOptions"));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var proxyServer = serviceProvider.GetService<ProxyServer>();

            var task = proxyServer.StartAsync();

            Task.WaitAll(task);
        }

        static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
            //.SetBasePath(Path.Combine(AppContext.BaseDirectory))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}