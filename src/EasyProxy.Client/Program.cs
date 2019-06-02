using EasyProxy.Core;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;


namespace EasyProxy.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ProxyClient>();
            var configuration = BuildConfiguration();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddLogging(configure => configure.AddConsole());
            serviceCollection.AddSingleton<ConfigHelper>();
            serviceCollection.Configure<ClientOptions>(configuration.GetSection("ClientOptions"));
            serviceCollection.AddSingleton(typeof(IPackageEncoder<ProxyPackage>), typeof(ProxyPackageEncoder));
            serviceCollection.AddSingleton(typeof(IPackageDecoder<ProxyPackage>), typeof(ProxyPackageDecoder));

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var client = serviceProvider.GetService<ProxyClient>();

            await client.StartAsync();

            //Task.WaitAll(task);

            Console.ReadKey();
        }

        static IConfiguration BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}
