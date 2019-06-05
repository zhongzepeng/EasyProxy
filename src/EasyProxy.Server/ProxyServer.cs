using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Common;
using EasyProxy.Core.Config;
using EasyProxy.Core.Model;
using EasyProxy.Server.Dashboard;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    public class ProxyServer : IProxyHost
    {
        private readonly ILogger<ProxyServer> logger;

        private readonly ServerOptions options;

        private readonly ProxyPackageDecoder decoder;
        private readonly ProxyPackageEncoder encoder;
        private readonly ConfigHelper configHelper;
        private readonly IIdGenerator idGenerator;

        public ProxyServer(IOptions<ServerOptions> options, ILogger<ProxyServer> logger, ProxyPackageDecoder decoder, ProxyPackageEncoder encoder, IIdGenerator idGenerator)
        {
            this.logger = logger;
            this.options = options?.Value;
            Checker.NotNull(this.options);
            this.decoder = decoder;
            this.encoder = encoder;
            configHelper = new ConfigHelper();
            this.idGenerator = idGenerator;
        }

        public async Task StartAsync()
        {
            if (options.EanbleDashboard)
            {
                _ = StartDashboardAsync();
            }
            await StartProxyServer();
        }

        private async Task StartProxyServer()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endpoint = new IPEndPoint(IPAddress.Any, options.ServerPort);
            socket.Bind(endpoint);
            socket.Listen(options.MaxConnection);
            await Task.Factory.StartNew(async () =>
            {
                logger.LogInformation($"ProxyServer listen on : {endpoint.Port}");
                while (true)
                {
                    var clientSocket = await socket.AcceptAsync();
                    var proxyChannel = new ProxyChannel<ProxyPackage>(clientSocket, encoder, decoder, logger, new ChannelOptions());
                    proxyChannel.PackageReceived += OnPackageReceived;
                    _ = proxyChannel.StartAsync();
                }
            });
        }

        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            switch (package.Type)
            {
                case PackageType.Connect:
                    await ProcessConnect(channel, package);
                    break;
                case PackageType.DisConnected:
                    await ProcessDisConnected(package);
                    break;
                case PackageType.Authentication:
                    await ProcessAuthentication(channel, package);
                    break;
            }
        }

        private async Task ProcessAuthentication(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            var model = package.Data.BytesToObject<AuthenticationModel>();

            var pass = await configHelper.CheckClientAsync(model.ClientId, model.SecretKey);

            if (!pass)
            {
                await channel.SendAsync(new ProxyPackage
                {
                    Data = new AuthenticationResult
                    {
                        Success = false,
                        Message = "ClientId not exits or SecretKey not correct"
                    }.ObjectToBytes(),
                    Type = PackageType.Authentication
                });
            }
            else
            {
                await channel.SendAsync(new ProxyPackage
                {
                    Data = new AuthenticationResult
                    {
                        Success = true,
                        Channels = await configHelper.GetChannelsAsync(model.ClientId)
                    }.ObjectToBytes(),
                    Type = PackageType.Authentication
                });
            }
        }

        private async Task ProcessConnect(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            var channelConfig = await configHelper.GetChannelAsync(package.ChannelId);
            ProxyServerChannelManager.AddChannel(package.ChannelId, channel);
            var connection = new ProxyServerConnection(package.ChannelId, channel, channelConfig.BackendPort, logger, idGenerator);
            await connection.StartAsync();
        }

        private async Task ProcessDisConnected(ProxyPackage package)
        {
            ProxyServerChannelManager.RemoveChannel(package.ChannelId);
            await Task.CompletedTask;
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
