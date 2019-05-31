using EasyProxy.Core;
using EasyProxy.Core.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClientConnection : IConnection
    {
        private readonly ProxyConfigurationItem proxyConfiguration;
        private readonly ILogger logger;
        private readonly IPAddress serverAddress;
        public ProxyClientConnection(ProxyConfigurationItem proxyConfiguration, ILogger logger, IPAddress serverAddress)
        {
            this.proxyConfiguration = proxyConfiguration;
            this.logger = logger;
            this.serverAddress = serverAddress;
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(serverAddress, proxyConfiguration.ServerPort);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await serverSocket.ConnectAsync(endpoint);

            var targetEndpoint = new IPEndPoint(IPAddress.Parse(proxyConfiguration.BackendAddress), proxyConfiguration.BackendProt);
            var targetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await targetSocket.ConnectAsync(targetEndpoint);

            var serverPipe = new Pipe();

            _ = PipelineUtils.FillPipeAsync(serverSocket, serverPipe.Writer);
            _ = PipelineUtils.ReadPipeAsync(serverPipe.Reader, targetSocket);

            var targetPipe = new Pipe();

            _ = PipelineUtils.FillPipeAsync(targetSocket, targetPipe.Writer);
            _ = PipelineUtils.ReadPipeAsync(targetPipe.Reader, serverSocket);

            logger.LogInformation($"链接成功：{proxyConfiguration.BackendAddress}:{proxyConfiguration.BackendProt}");
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
