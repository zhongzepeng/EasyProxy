using EasyProxy.Core;
using EasyProxy.Core.Common;
using EasyProxy.Server.Dashboard;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    public class ProxyServer : IProxyHost
    {
        private readonly ILogger<ProxyServer> logger;

        private readonly ServerOptions options;

        private readonly CancellationToken cancellationToken;

        private readonly ConcurrentDictionary<int, ProxyConfigurationItem> onlineConnections;

        public ProxyServer(IOptions<ServerOptions> options, ILogger<ProxyServer> logger)
        {
            this.logger = logger;
            this.options = options?.Value;
            Checker.NotNull(this.options);
            cancellationToken = new CancellationTokenSource().Token;
            onlineConnections = new ConcurrentDictionary<int, ProxyConfigurationItem>();
        }

        public async Task StartAsync()
        {
            if (options.EanbleDashboard)
            {
                await StartDashboardAsync();
            }
            await StartProxyServerAsync();
        }

        private async Task StartProxyServerAsync()
        {
            var connections = ProxyConfigurationHelper.GetAllConnections();
            foreach (var connection in connections)
            {
                try
                {
                    var proxyServerConnection = new ProxyServerConnection(connection, logger);
                    await proxyServerConnection.StartAsync();
                    onlineConnections.TryAdd(connection.Id, connection);
                    logger.LogInformation($"Connection {connection.Id} start success!");
                }
                catch (Exception ex)
                {
                    logger.LogInformation($"{ex} Connection {connection.Id} start fail!");
                }
            }
        }


        private async Task StartDashboardAsync()
        {
            logger.LogInformation($"Start Dashboard Server on :{options.DashboardPort}");
            await WebHost.CreateDefaultBuilder().UseStartup<Startup>().Build().RunAsync();
        }

        public Task StopAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
