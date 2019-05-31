using EasyProxy.Core;
using EasyProxy.Core.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClient : IProxyHost
    {
        private readonly ILogger<ProxyClient> logger;

        private readonly ClientOptions options;

        public ProxyClient(IOptions<ClientOptions> options, ILogger<ProxyClient> logger)
        {
            this.logger = logger;
            this.options = options?.Value;
            Checker.NotNull(this.options);
        }

        public async Task StartAsync()
        {
            var connections = ProxyConfigurationHelper.GetConnectionByClientId(Guid.Empty);
            foreach (var conn in connections)
            {
                var clientConn = new ProxyClientConnection(conn, logger, IPAddress.Parse(options.ServerAddress));
                _ = clientConn.StartAsync();
            }
            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
