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
        private readonly ChannelOptions channelOptions;
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
            channelOptions = new ChannelOptions();
            serverChannelHolder = new Dictionary<long, IChannel>();
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(serverAddress, serverPort);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await serverSocket.ConnectAsync(endpoint);
            proxyChannel = new ProxyChannel<ProxyPackage>(serverSocket, encoder, decoder, logger, channelOptions);
            proxyChannel.PackageReceived += OnPackageReceived;
            proxyChannel.Closed += OnProxyChannelClosed;
            var task = proxyChannel.StartAsync();
            await proxyChannel.SendAsync(new ProxyPackage
            {
                ChannelId = channelConfig.ChannelId,
                Type = PackageType.Connect
            });

            await task;
        }
        private async Task OnProxyChannelClosed(IChannel channel)
        {
            var pchannel = channel as ProxyChannel<ProxyPackage>;
            await SendDisconnectPackage(pchannel, -1);
        }

        public async Task StopAsync()
        {
            var channels = serverChannelHolder.Values;
            foreach (var channel in channels)
            {
                await channel.Close();
            }
            await proxyChannel.Close();

            await Task.CompletedTask;
        }

        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            switch (package.Type)
            {
                case PackageType.Transfer:
                    await ProcessTransfer(channel, package);
                    break;
                case PackageType.Disconnect:
                    await ProcessDisconect(channel, package);
                    break;
            }
        }
        private async Task ProcessDisconect(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            if (serverChannelHolder.ContainsKey(package.ConnectionId))
            {
                //logger.LogInformation("收到服务端发送的断开连接");
                await serverChannelHolder[package.ConnectionId].Close();
                serverChannelHolder.Remove(package.ConnectionId);
            }
            await Task.CompletedTask;
        }
        private async Task ProcessTransfer(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            IChannel targetChannel;
            if (!serverChannelHolder.ContainsKey(package.ConnectionId))
            {
                var targetEp = new IPEndPoint(IPAddress.Parse(channelConfig.FrontendIp), channelConfig.FrontendPort);
                var wrapper = new TimeoutSocketWrapper(targetEp);
                var nsocket = wrapper.Connect(channelOptions.ConnectTimeout);
                if (nsocket == null)
                {
                    await SendDisconnectPackage(channel, package.ConnectionId);
                    return;
                }
                var connectionId = package.ConnectionId;
                targetChannel = new MarkedProxyChannel(connectionId, nsocket, logger, channelOptions);
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
            await proxyChannel.SendAsync(package);
        }


        /// <summary>
        /// connectionId -1 表示关闭整个channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        private async Task SendDisconnectPackage(IChannel<ProxyPackage> channel, long connectionId)
        {
            await channel.SendAsync(new ProxyPackage
            {
                Type = PackageType.Disconnect,
                ChannelId = channelConfig.ChannelId,
                ConnectionId = connectionId
            });
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
