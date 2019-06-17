using EasyProxy.Core.Channel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public class EasyHttpServer : IHttpServer
    {
        const int Backlog = 100;
        private readonly Socket serverSocket;
        private readonly ILogger<EasyHttpServer> logger;
        private HttpServerOptions options;
        private IHttpHandler httpHandler;
        public event Func<HttpRequest, Exception, Task<HttpResponse>> RequestError;
        public EasyHttpServer(IOptions<HttpServerOptions> options, ILogger<EasyHttpServer> logger, IHttpHandler httpHandler)
        {
            this.httpHandler = httpHandler;
            this.options = options.Value;
            this.logger = logger;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse(this.options.Address), this.options.Port);
            serverSocket.Bind(endpoint);
        }

        protected Task<HttpResponse> OnRequestError(HttpRequest request, Exception exception)
        {
            return RequestError?.Invoke(request, exception);
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
                    //httpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    logger.LogDebug($"accept new {httpSocket.RemoteEndPoint}");
                    var httpChannel = new HttpChannel(httpSocket, logger, new ChannelOptions());
                    httpChannel.HttpRequested += OnHttpRequested;
                    _ = httpChannel.StartAsync();
                }
            });
        }

        private async Task OnHttpRequested(HttpChannel channel, HttpRequest request)
        {
            logger.LogDebug($"httprequest:{request}");
            HttpResponse httpResponse;
            try
            {
                httpResponse = await httpHandler.ProcessAsync(request);
            }
            catch (Exception e)
            {
                logger.LogError("Exception throw ProccessAsync", e);
                httpResponse = await OnRequestError(request, e);
            }
            if (request.Headers.ContainsKey("Connection"))
            {
                httpResponse.Headers.Add("Connection", request.Headers["Connection"]);
            }
            var res = httpResponse.ToHttpProtocolData();
            await channel.SendAsync(res);
        }
    }
}
