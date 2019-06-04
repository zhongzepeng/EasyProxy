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
                    logger.LogInformation($"服务端接收道请求：{clientSocket.RemoteEndPoint},connectionId:{connectionId}");
                    var channel = new MarkedProxyChannel(connectionId, clientSocket, logger, new ChannelOptions());

                    channel.DataReceived += OnDataReceived;

                    clientChannelHolder.TryAdd(connectionId, channel);

                    _ = channel.StartAsync();
                }
            });
        }

        private async Task<SequencePosition> OnDataReceived(IChannel channel, ReadOnlySequence<byte> buffer)
        {
            var markedChannel = channel as MarkedProxyChannel;
            logger.LogWarning($"服务器接收到外网数据：{markedChannel.Mark}");
            var total = buffer.Length;
            var data = buffer.Slice(0, total).ToArray();
            await TransferAsync(markedChannel.Mark, data);
            return buffer.GetPosition(total);
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
        }

        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
        {
            if (package.Type != PackageType.Transfer)
                return;
            var existsChannel = clientChannelHolder[package.ConnectionId];
            logger.LogInformation($"收到客户端发送的数据包：{package}");
            await existsChannel.SendAsync(package.Data);
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
            logger.LogInformation($"服务端发送数据包到客户端：{package}");
            await channel.SendAsync(package);
            //await Task.CompletedTask;
        }
    }
}
