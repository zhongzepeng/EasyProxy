using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

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

        public async Task<int> EncodeAsync(PipeWriter writer, ProxyPackage package)
        {
            var frameLength = ProxyPackage.TYPE_SIZE + ProxyPackage.CHANNELID_SIZE + ProxyPackage.CONNECTIONID_SIZE + package.Data.Length;
            await writer.WriteAsync(BitConverter.GetBytes(frameLength));
            await writer.WriteAsync(new byte[] { (byte)package.Type });
            await writer.WriteAsync(BitConverter.GetBytes(package.ConnectionId));
            await writer.WriteAsync(BitConverter.GetBytes(package.ChannelId));
            await writer.WriteAsync(package.Data);
            return frameLength;
        }
    }
}
