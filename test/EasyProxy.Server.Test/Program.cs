using EasyProxy.Core;
using EasyProxy.Core.Channel;
using EasyProxy.Core.Codec;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Server.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var encoder = new ProxyPackageEncoder();
            var decoder = new ProxyPackageDecoder();
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var endpoint = new IPEndPoint(IPAddress.Any, 9091);

            socket.Bind(endpoint);

            socket.Listen(10);
            Console.WriteLine($"开始监听：{endpoint}");
            var factory = new LoggerFactory();
            var logger = factory.CreateLogger("");
            while (true)
            {
                var nscoket = await socket.AcceptAsync();
                Console.WriteLine($"连接成功：{nscoket.RemoteEndPoint}");
                var channel = new ProxyChannel<ProxyPackage>(nscoket, encoder, decoder, logger, new ChannelOptions());
                channel.PackageReceived += async (channl, package) =>
                {
                    Console.WriteLine($"接收到一个数据包：{package},内容：{Encoding.UTF8.GetString(package.Data)}");
                    var reply = new ProxyPackage
                    {
                        Data = Encoding.UTF8.GetBytes("你好呀")
                    };
                    await channel.SendAsync(package);
                };
                await channel.StartAsync();
            }
        }
    }
}
