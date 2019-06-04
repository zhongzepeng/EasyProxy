using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace EasyProxy.Core.Codec
{
    public class ProxyPackageEncoder : IPackageEncoder<ProxyPackage>
    {
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
