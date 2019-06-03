using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClientConnection : IConnection
    {
        const int BUFFER_SIZE = 1024;
        private readonly Dictionary<long, Socket> serverSocketHolder;
        private readonly ILogger logger;
        private readonly IPAddress serverAddress;
        private readonly ChannelConfig channelConfig;
        private readonly int serverPort;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        private TcpPipeChannel<ProxyPackage> proxyChannel;
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
            serverSocketHolder = new Dictionary<long, Socket>();
        }

        public async Task StartAsync()
        {
            var endpoint = new IPEndPoint(serverAddress, serverPort);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await serverSocket.ConnectAsync(endpoint);
            logger.LogInformation($"channel start :{endpoint}");
            proxyChannel = new TcpPipeChannel<ProxyPackage>(serverSocket, logger, encoder, decoder);

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
            Socket nsocket;
            if (!serverSocketHolder.ContainsKey(package.ConnectionId))
            {
                nsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var targetEp = new IPEndPoint(IPAddress.Parse(channelConfig.FrontendIp), channelConfig.FrontendPort);
                await nsocket.ConnectAsync(targetEp);
                var connectionId = package.ConnectionId;
                logger.LogInformation($"与目标服务器建立连接:{connectionId},{targetEp.Port}");
                serverSocketHolder[package.ConnectionId] = nsocket;
                var pipe = new Pipe();
                _ = PipelineUtils.FillPipeAsync(nsocket, pipe.Writer);
                //将目标服务器的回复转发给服务器
                _ = ReadPipeAsync(pipe.Reader, BUFFER_SIZE, connectionId);
            }
            else
            {
                nsocket = serverSocketHolder[package.ConnectionId];
            }

            _ = nsocket.SendAsync(package.Data, SocketFlags.None);
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
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
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

                var examined = buffer.GetPosition(total);

                pipeReader.AdvanceTo(buffer.Start, examined);

                await TransferAsync(connectionId, array);

                if (isCompleted)
                { break; }
            }

            pipeReader.Complete();
        }
    }
}
