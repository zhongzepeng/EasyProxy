namespace EasyProxy.Core
{
    public class ProxyPackage
    {
        public const int HEADER_SIZE = 4;
        public const int TYPE_SIZE = 1;
        public const int CHANNELID_SIZE = 4;
        public const int CONNECTIONID_SIZE = 8;
        public PackageType Type { get; set; }
        public int ChannelId { get; set; }
        public long ConnectionId { get; set; }
        public byte[] Data { get; set; } = new byte[0];

        public override string ToString() => $"Type:{Type},ChannelId:{ChannelId},ConnectionId:{ConnectionId},DataLength:{Data.Length}";
    }

    public enum PackageType
    {
        /// <summary>
        /// 客户端与服务端建立连接
        /// </summary>
        Connect = 0x00,
        /// <summary>
        /// 客户端，服务端传输数据
        /// </summary>
        Transfer = 0x01,
        /// <summary>
        /// 断开连接
        /// </summary>
        Disconnect = 0x02,
        /// <summary>
        /// 客户端认证【认证成功后，返回当前clientId的所有channel配置】 
        /// </summary>
        Authentication = 0x03
    }
}
