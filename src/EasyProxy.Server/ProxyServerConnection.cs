using EasyProxy.Core;
using EasyProxy.Core.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    public class ProxyServerConnection : IConnection
    {
        private readonly ProxyConfigurationItem proxyConfiguration;
        private readonly ILogger logger;
        private readonly CancellationToken cancellationToken;
        public ProxyServerConnection(ProxyConfigurationItem proxyConfiguration, ILogger logger)
        {
            this.proxyConfiguration = proxyConfiguration;
            cancellationToken = new CancellationTokenSource().Token;
            this.logger = logger;
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, proxyConfiguration.ServerPort);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(endpoint);

            serverSocket.Listen(proxyConfiguration.MaxConnection);

            logger.LogInformation($"Proxy server Linten on : {endpoint.Port}");

            _ = Task.Factory.StartNew(async () =>
              {
                  while (true)
                  {
                      if (cancellationToken.IsCancellationRequested)
                      {
                          break;
                      }
                      var socket = await serverSocket.AcceptAsync();
                      var targetScoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                      var connection = ProxyConfigurationHelper.GetConnectionByServerPort(proxyConfiguration.ServerPort);
                      var toPipe = new Pipe();
                      _ = PipelineUtils.FillPipeAsync(socket, toPipe.Writer);

                      //TODO:处理和客户端进行交互，将客户端的数据返回给外网机器
                  }

              });
            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
