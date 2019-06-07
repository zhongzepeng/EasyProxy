using EasyProxy.Core.Channel;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        private HttpServerOptions options;
        private IHttpHandler httpHandler;
        public event Func<HttpRequest, Exception, Task> RequestError;
        public EasyHttpServer(HttpServerOptions options, ILogger logger, IHttpHandler httpHandler = null)
        {
            this.httpHandler = httpHandler;
            if (this.httpHandler == null)
            {
                this.httpHandler = new DefaultHttpHandler();
            }
            this.options = options;
            this.logger = logger;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Parse(options.Address), options.Port);
            serverSocket.Bind(endpoint);
        }

        protected void OnRequestError(HttpRequest request, Exception exception)
        {
            RequestError?.Invoke(request, exception);
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
                    httpChannel.HttpRequested += OnHttpRequested;
                    _ = httpChannel.StartAsync();
                }
            });
        }


        private async Task OnHttpRequested(HttpChannel channel, HttpRequest request)
        {
            HttpResponse httpResponse;
            try
            {
                httpResponse = await httpHandler.ProcessAsync(request);
            }
            catch (Exception e)
            {
                httpResponse = HttpResponseHelper.CreateDefaultErrorResponse();
                OnRequestError(request, e);
            }
            var res = httpResponse.ToHttpProtocolData();
            await channel.SendAsync(res);
        }
    }
}
