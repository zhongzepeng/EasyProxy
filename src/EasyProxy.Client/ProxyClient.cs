using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Common;
using EasyProxy.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClient : IProxyHost
    {
        private readonly ILogger<ProxyClient> logger;
        private readonly ClientOptions options;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        private readonly ChannelOptions channelOptions;
        public ProxyClient(IOptions<ClientOptions> options
            , ILogger<ProxyClient> logger
            , IPackageEncoder<ProxyPackage> encoder
            , IPackageDecoder<ProxyPackage> decoder)
        {
            this.logger = logger;
            this.options = options?.Value;
            Checker.NotNull(this.options);
            this.encoder = encoder;
            this.decoder = decoder;
            channelOptions = new ChannelOptions();
        }

        public async Task StartAsync()
        {
            await StartAuthenticationAsync();
        }

        private async Task StartAuthenticationAsync()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse(options.ServerAddress), options.ServerPort);
            var authSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await authSocket.ConnectAsync(endpoint);
            var authChannel = new ProxyChannel<ProxyPackage>(authSocket, encoder, decoder, logger, channelOptions);

            authChannel.PackageReceived += OnAuthPackageReceived;

            authChannel.Closed += OnAuthChannelClosed;

            _ = authChannel.StartAsync();

            await authChannel.SendAsync(new ProxyPackage
            {
                Type = PackageType.Authentication,
                Data = new AuthenticationModel
                {
                    ClientId = options.ClientId,
                    SecretKey = options.SecretKey
                }.ObjectToBytes()
            });
        }

        private async Task OnAuthPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            logger.LogInformation("授权消息");
            var result = package.Data.BytesToObject<AuthenticationResult>();

            if (!result.Success)
            {
                logger.LogInformation(result.Message);
                return;
            }
            var channels = result.Channels;
            if (channels.Count == 0)
            {
                logger.LogInformation("Not channel in config");
                return;
            }
            foreach (var c in channels)
            {
                var connection = new ProxyClientConnection(logger, IPAddress.Parse(options.ServerAddress), options.ServerPort, c, encoder, decoder);
                _ = connection.StartAsync();
            }
            await Task.CompletedTask;
        }

        private void OnAuthChannelClosed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
