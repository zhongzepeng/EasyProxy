namespace EasyProxy.Core
{
    public class ProxyPackage
    {
        public const int HEADER_SIZE = 4;
        public const int TYPE_SIZE = 1;
        public const int CONNECTIONID_SIZE = 4;

        public PackageType Type { get; set; }
        public int ConnectionId { get; set; }
        public byte[] Data { get; set; }
    }

    public enum PackageType
    {
        Register = 0x00,
        Transfer = 0x01
    }
}
