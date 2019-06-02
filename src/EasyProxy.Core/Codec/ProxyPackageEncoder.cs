using System;

namespace EasyProxy.Core.Codec
{
    public class ProxyPackageEncoder : IPackageEncoder<ProxyPackage>
    {
        public byte[] Encode(ProxyPackage package)
        {
            var frameLength = ProxyPackage.TYPE_SIZE + ProxyPackage.CHANNELID_SIZE + ProxyPackage.CONNECTIONID_SIZE + package.Data.Length;
            var total = frameLength + ProxyPackage.HEADER_SIZE;
            var bytes = new byte[total];

            var pos = 0;
            BitConverter.GetBytes(frameLength).CopyTo(bytes, pos);
            pos += ProxyPackage.HEADER_SIZE;

            bytes.SetValue((byte)package.Type, pos);
            pos += ProxyPackage.TYPE_SIZE;

            BitConverter.GetBytes(package.ConnectionId).CopyTo(bytes, pos);
            pos += ProxyPackage.CONNECTIONID_SIZE;

            BitConverter.GetBytes(package.ChannelId).CopyTo(bytes, pos);
            pos += ProxyPackage.CHANNELID_SIZE;

            package.Data.CopyTo(bytes, pos);

            return bytes;
        }
    }
}
