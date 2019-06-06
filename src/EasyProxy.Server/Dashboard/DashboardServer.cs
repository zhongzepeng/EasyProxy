//using EasyProxy.Core;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Logging;
//using System.Threading.Tasks;

//namespace EasyProxy.Server.Dashboard
//{
//    public class DashboardServer : IProxyHost
//    {
//        private readonly string host;
//        private readonly int serverPort;
//        private readonly ILogger logger;
//        private IWebHost webhost;
//        public DashboardServer(string host, int serverPort, ILogger logger)
//        {
//            this.logger = logger;
//            this.host = host;
//            this.serverPort = serverPort;
//        }

//        public Task StartAsync()
//        {
//            logger.LogInformation($"Dashboard server start on http://{host}:{serverPort}");
//            webhost = new WebHostBuilder()
//               .UseUrls($"http://{host}:{serverPort}")
//               .UseKestrel()
//               .UseStartup<Startup>()
//               .Build();
//            return webhost.RunAsync();
//        }

//        public async Task StopAsync()
//        {
//            await webhost.StopAsync();
//        }
//    }
//}
