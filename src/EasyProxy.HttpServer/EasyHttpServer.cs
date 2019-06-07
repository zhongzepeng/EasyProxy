using EasyProxy.Core.Channel;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public class EasyHttpServer : IHttpServer
    {
        const int Backlog = 100;
        private readonly Socket serverSocket;
        private readonly ILogger logger;
        private HttpServerOptions options;

        public EasyHttpServer(HttpServerOptions options, ILogger logger)
        {
            this.options = options;
            this.logger = logger;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse(options.Address), options.Port);
            serverSocket.Bind(endpoint);
        }

        public async Task ListenAsync()
        {
            serverSocket.Listen(Backlog);
            logger.LogInformation($"httpserver listen on {options.Port}");
            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var httpSocket = await serverSocket.AcceptAsync();
                    var httpChannel = new HttpChannel(httpSocket, logger, new ChannelOptions());
                    httpChannel.HttpRequested += async (channel, httpRequest) =>
                    {
                        logger.LogInformation(httpRequest.ToString());
                        await Task.CompletedTask;
                    };
                    _ = httpChannel.StartAsync();
                }
            });
        }
    }
}
