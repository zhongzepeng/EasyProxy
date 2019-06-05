using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Client.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var encoder = new ProxyPackageEncoder();
            var decoder = new ProxyPackageDecoder();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var endpoint = new IPEndPoint(IPAddress.Loopback, 9091);

            await socket.ConnectAsync(endpoint);
            var factory = new LoggerFactory();
            var logger = factory.CreateLogger("");
            var options = new ChannelOptions();
            var channel = new ProxyChannel<ProxyPackage>(socket, encoder, decoder, logger, options);

            channel.PackageReceived += async (channl, package) =>
            {
                Console.WriteLine($"接收到一个数据包：{package},内容：{Encoding.UTF8.GetString(package.Data)}");
                await Task.CompletedTask;
            };

            _ = channel.StartAsync();
            var c = 0;
            while (c < 100000)
            {
                var package = new ProxyPackage
                {
                    ChannelId = 1,
                    ConnectionId = 100,
                    Type = PackageType.Transfer,
                    Data = Encoding.UTF8.GetBytes(c.ToString())
                };

                await channel.SendAsync(package);

                c++;
            }

            Console.ReadKey();
        }
    }
}
