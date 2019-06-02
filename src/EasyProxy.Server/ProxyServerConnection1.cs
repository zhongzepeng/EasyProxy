//using EasyProxy.Core;
//using EasyProxy.Core.Channel;
//using EasyProxy.Core.Codec;
//using EasyProxy.Core.Config;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading.Tasks;

//namespace EasyProxy.Server
//{
//    /// <summary>
//    /// 负责客户端注册，和数据传输
//    /// </summary>
//    public class ProxyServerConnection : IConnection
//    {

//        private readonly ILogger logger;
//        private readonly int serverPort;
//        private readonly Socket socket;
//        private readonly int backlog;
//        private readonly ProxyPackageEncoder encoder;
//        private readonly ProxyPackageDecoder decoder;
//        private readonly ConfigHelper configHelper;

//        public ProxyServerConnection(ILogger logger, int serverPort, int backlog, ProxyPackageDecoder decoder, ProxyPackageEncoder encoder)
//        {
//            this.serverPort = serverPort;
//            this.logger = logger;
//            this.backlog = backlog;
//            this.encoder = encoder;
//            this.decoder = decoder;
//            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            configHelper = new ConfigHelper();
//        }

//        public async Task StartAsync()
//        {
//            var endpoint = new IPEndPoint(IPAddress.Any, serverPort);
//            socket.Bind(endpoint);
//            socket.Listen(backlog);
//            await Task.Factory.StartNew(async () =>
//           {
//               logger.LogInformation($"ProxyServer listen on : {endpoint.Port}");
//               while (true)
//               {
//                   var clientSocket = await socket.AcceptAsync();
//                   var proxyChannel = new ProxyChannel(clientSocket, encoder, decoder);
//                   proxyChannel.PackageReceived += OnPackageReceived;
//                   _ = proxyChannel.StartAsync();
//               }
//           });
//        }

//        private async Task OnPackageReceived(IChannel<ProxyPackage> channel, ProxyPackage package)
//        {
//            switch (package.Type)
//            {
//                case PackageType.Connect:
//                    var channelConfig = await configHelper.GetChannelAsync(package.ChannelId);
//                    ProxyServerChannelManager.AddChannel(package.ChannelId, channel);
//                    break;

//                case PackageType.DisConnected:
//                    ProxyServerChannelManager.RemoveChannel(package.ChannelId);
//                    break;

//                case PackageType.Transfer:
//                    break;
//            }
//        }

//        public Task StopAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
