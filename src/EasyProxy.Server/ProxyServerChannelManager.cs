using EasyProxy.Core;
using EasyProxy.Core.Channel;
using System.Collections.Concurrent;

namespace EasyProxy.Server
{
    /// <summary>
    /// 管理服务端和客户端的连接
    /// </summary>
    public class ProxyServerChannelManager
    {
        private readonly static ConcurrentDictionary<int, IChannel<ProxyPackage>> channelHolder = new ConcurrentDictionary<int, IChannel<ProxyPackage>>();

        public static bool AddChannel(int channelId, IChannel<ProxyPackage> channel)
        {
            return channelHolder.TryAdd(channelId, channel);
        }

        public static bool RemoveChannel(int chanelId)
        {
            return channelHolder.TryRemove(chanelId, out _);
        }
    }
}
