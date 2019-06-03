using System.IO.Pipelines;
using System.Threading.Tasks;

namespace EasyProxy.Core.Codec
{
    public interface IPackageEncoder<in TPackageInfo> where TPackageInfo : class
    {
        byte[] Encode(TPackageInfo package);

        Task<int> EncodeAsync(PipeWriter writer, TPackageInfo package);
    }
}