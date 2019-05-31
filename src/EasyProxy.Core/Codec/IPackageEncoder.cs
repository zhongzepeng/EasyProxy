using System;

namespace EasyProxy.Core.Codec
{
    public interface IPackageEncoder<in TPackageInfo> where TPackageInfo : class
    {
        ReadOnlyMemory<byte> Encode(TPackageInfo package);
    }
}