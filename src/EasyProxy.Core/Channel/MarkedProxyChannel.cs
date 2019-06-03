using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace EasyProxy.Core.Channel
{
    public class MarkedProxyChannel : ProxyChannel
    {
        public long Mark { get; }

        public MarkedProxyChannel(long mark, Socket socket, ILogger logger, ChannelOptions options) : base(socket, logger, options)
        {
            Mark = mark;
        }
    }
}
