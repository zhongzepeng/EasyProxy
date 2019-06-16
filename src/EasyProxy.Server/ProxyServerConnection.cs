using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    public class ProxyServerConnection : IConnection
    {
        private readonly Socket socket;
        private readonly int backendPort;
        private readonly ILogger logger;
        private readonly IIdGenerator idGenerator;
        private readonly IChannel<ProxyPackage> channel;
        private readonly int channelId;
        private readonly ConcurrentDictionary<long, IChannel> clientChannelHolder;
        public ProxyServerConnection(int channelId, IChannel<ProxyPackage> channel, int port, ILogger logger, IIdGenerator idGenerator)
        {
            this.channelId = channelId;
            this.channel = channel;
            this.channel.Closed += OnChannelClosed;
            this.channel.PackageReceived += OnPackageReceived;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            backendPort = port;
            this.logger = logger;
            this.idGenerator = idGenerator;
            clientChannelHolder = new ConcurrentDictionary<long, IChannel>();
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, backendPort);
            socket.Bind(endpoint);
            socket.Listen(200);

            await Task.Factory.StartNew(async () =>
            {
                logger.LogInformation($"listen on : {endpoint.Port}");

                while (true)
                {
                    var clientSocket = await socket.AcceptAsync();
                    var connectionId = idGenerator.Next();
                    logger.LogInformation($"connectionId:{connectionId}");
                    var channel = new MarkedProxyChannel(connectionId, clientSocket, logger, new ChannelOptions());
                    channel.DataReceived += OnDataReceived;
                    channel.Closed += OnMarkedProxyChannelClosed;
                    _ = channel.StartAsync();
                    clientChannelHolder.TryAdd(connectionId, channel);
                }
            });
        }

        private void OnMarkedProxyChannelClosed(object sender, EventArgs e)
        {
            var channel = sender as MarkedProxyChannel;
            channel.Close();
            clientChannelHolder.TryRemove(channel.Mark, out _);
        }  

        private async Task<SequencePosition> OnDataReceived(IChannel channel, ReadOnlySequence<byte> buffer)
        {
            var markedChannel = channel as MarkedProxyChannel;
            var total = buffer.Length;
            var data = buffer.Slice(0, total).ToArray();
            await TransferAsync(markedChannel.Mark, data);
            return buffer.GetPosition(total);
        }

        public async Task StopAsync()
        {
            await Task.CompletedTask;
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
        }

        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            switch (package.Type)
            {
                case PackageType.Transfer:
                    await ProcessTransfer(channel, package);
                    break;
                case PackageType.Disconnect:
                    await ProcessDisconnect(channel, package);
                    break;
            }
        }

        private async Task ProcessTransfer(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            var existsChannel = clientChannelHolder[package.ConnectionId];
            await existsChannel.SendAsync(package.Data);
        }

        private async Task ProcessDisconnect(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            var existsChannel = clientChannelHolder[package.ConnectionId];
            existsChannel?.Close();
            await Task.CompletedTask;
        }

        private async Task TransferAsync(long connectionId, byte[] data)
        {
            var package = new ProxyPackage
            {
                ChannelId = channelId,
                ConnectionId = connectionId,
                Data = data,
                Type = PackageType.Transfer
            };
            await channel.SendAsync(package);
        }
    }
}
