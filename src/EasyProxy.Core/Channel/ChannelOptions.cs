namespace EasyProxy.Core.Channel
{
    public class ChannelOptions
    {
        public int MaxPackageLength { get; set; } = 1024 * 1024 * 4;

        public int ReceiveBufferSize { get; set; } = 1024 * 4;

        public int ConnectTimeout { get; set; } = 3000;
    }
}
