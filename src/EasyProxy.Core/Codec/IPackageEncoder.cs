using System;

namespace EasyProxy.Core.Codec
{
    public interface IPackageEncoder<in TPackageInfo> where TPackageInfo : class
    {
        byte[] Encode(TPackageInfo package);
    }
}