using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace EasyProxy.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ProxyClient>();
            var configuration = BuildConfiguration();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(configure => configure.AddConsole());
            serviceCollection.Configure<ClientOptions>(configuration.GetSection("ClientOptions"));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var client = serviceProvider.GetService<ProxyClient>();

            var task = client.StartAsync();

            Task.WaitAll(task);
        }

        static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}
