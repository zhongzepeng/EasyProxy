using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Server
{
    public class ProxyServerConnection : IConnection
    {
        const int BUFFER_SIZE = 1024;
        private readonly Socket socket;
        private readonly int backendPort;
        private readonly ILogger logger;
        private readonly IIdGenerator idGenerator;
        private readonly IChannel<ProxyPackage> channel;
        private readonly int channelId;
        private readonly ConcurrentDictionary<long, Socket> clientSocketHolder;
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
            clientSocketHolder = new ConcurrentDictionary<long, Socket>();
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(IPAddress.Any, backendPort);
            socket.Bind(endpoint);
            socket.Listen(20);

            await Task.Factory.StartNew(async () =>
            {
                logger.LogInformation($"listen on : {endpoint.Port}");

                while (true)
                {
                    var inputPipe = new Pipe();
                    var clientSocket = await socket.AcceptAsync();
                    var connectionId = idGenerator.Next();
                    logger.LogInformation($"服务端接收道请求：{clientSocket.RemoteEndPoint},connectionId:{connectionId},socketCount:{clientSocketHolder.Count + 1}");
                    clientSocketHolder.TryAdd(connectionId, clientSocket);
                    _ = PipelineUtils.FillPipeAsync(clientSocket, inputPipe.Writer);

                    _ = ReadPipeAsync(inputPipe.Reader, BUFFER_SIZE, connectionId);

                }
            });
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
            var socket = clientSocketHolder[package.ConnectionId];
            logger.LogInformation($"收到客户端发送的数据包：{package.Data.Length},{socket.RemoteEndPoint},{package.ConnectionId}");
            await socket.SendAsync(package.Data, SocketFlags.None);
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
            logger.LogInformation($"发送数据到客户端，connectionId：{connectionId},length：{data.Length}");
            await channel.SendAsync(package);
        }

        private async Task ReadPipeAsync(PipeReader pipeReader, int bufferLength, long connectionId)
        {
            while (true)
            {
                var result = await pipeReader.ReadAsync();

                if (result.IsCanceled)
                {
                    break;
                }
                var isCompleted = result.IsCompleted;

                var buffer = result.Buffer;

                var total = Math.Min(bufferLength, buffer.Length);

                var array = buffer.Slice(0, total).ToArray();

                if (array.Length == 0)
                {
                    continue;
                }

                await TransferAsync(connectionId, array);

                var examined = buffer.GetPosition(total);

                pipeReader.AdvanceTo(buffer.Start, examined);

                if (isCompleted)
                { break; }
            }

            pipeReader.Complete();
        }
    }
}
