using System;

namespace EasyProxy.Core.Codec
{
    public class ProxyPackageEncoder : IPackageEncoder<ProxyPackage>
    {
        public ReadOnlyMemory<byte> Encode(ProxyPackage package)
        {
            var total = ProxyPackage.HEADER_SIZE + ProxyPackage.TYPE_SIZE + ProxyPackage.CONNECTIONID_SIZE + package.Data.Length;
            var bytes = new byte[total];
            throw new NotImplementedException();
        }
    }
}
