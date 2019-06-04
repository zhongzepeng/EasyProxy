using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClientConnection : IConnection
    {
        private readonly Dictionary<long, IChannel> serverChannelHolder;
        private readonly ILogger logger;
        private readonly IPAddress serverAddress;
        private readonly ChannelConfig channelConfig;
        private readonly int serverPort;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        private ProxyChannel<ProxyPackage> proxyChannel;
        public ProxyClientConnection(ILogger logger
            , IPAddress serverAddress
            , int serverPort
            , ChannelConfig channelConfig
            , IPackageEncoder<ProxyPackage> encoder
            , IPackageDecoder<ProxyPackage> decoder)
        {
            this.serverPort = serverPort;
            this.channelConfig = channelConfig;
            this.logger = logger;
            this.serverAddress = serverAddress;
            this.encoder = encoder;
            this.decoder = decoder;
            serverChannelHolder = new Dictionary<long, IChannel>();
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(serverAddress, serverPort);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var options = new ChannelOptions();
            await serverSocket.ConnectAsync(endpoint);
            logger.LogInformation($"channel start :{endpoint}");
            proxyChannel = new ProxyChannel<ProxyPackage>(serverSocket, encoder, decoder, logger, options);

            proxyChannel.PackageReceived += OnPackageReceived;
            proxyChannel.Closed += OnChannelClosed;

            _ = proxyChannel.StartAsync();

            await proxyChannel.SendAsync(new ProxyPackage
            {
                ChannelId = channelConfig.ChannelId,
                Type = PackageType.Connect
            });
            logger.LogInformation("channel start");
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 收到服务端转发的数据，直接发给目标服务器
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            if (package.Type != PackageType.Transfer)
                return;
            logger.LogWarning($"客户端接收道数据包：{package}");
            IChannel targetChannel;
            if (!serverChannelHolder.ContainsKey(package.ConnectionId))
            {
                var nsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var targetEp = new IPEndPoint(IPAddress.Parse(channelConfig.FrontendIp), channelConfig.FrontendPort);
                await nsocket.ConnectAsync(targetEp);
                var connectionId = package.ConnectionId;
                logger.LogInformation($"与目标服务器建立连接:{connectionId},{targetEp.Port}");
                targetChannel = new MarkedProxyChannel(connectionId, nsocket, logger, new ChannelOptions());
                targetChannel.DataReceived += OnDataReceived;
                serverChannelHolder[package.ConnectionId] = targetChannel;
                _ = targetChannel.StartAsync();
            }
            else
            {
                targetChannel = serverChannelHolder[package.ConnectionId];
            }

            await targetChannel.SendAsync(package.Data);
        }

        private async Task TransferAsync(long connectionId, byte[] data)
        {
            var package = new ProxyPackage
            {
                ChannelId = channelConfig.ChannelId,
                ConnectionId = connectionId,
                Data = data,
                Type = PackageType.Transfer
            };
            logger.LogInformation($"发送数据包到服务端：{package}");
            await proxyChannel.SendAsync(package);
            //await Task.CompletedTask;
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
        }

        private async Task<SequencePosition> OnDataReceived(IChannel channel, ReadOnlySequence<byte> buffer)
        {
            var markedChannel = channel as MarkedProxyChannel;
            var total = buffer.Length;
            var data = buffer.Slice(0, total).ToArray();
            await TransferAsync(markedChannel.Mark, data);
            return buffer.GetPosition(total);
        }
    }
}
