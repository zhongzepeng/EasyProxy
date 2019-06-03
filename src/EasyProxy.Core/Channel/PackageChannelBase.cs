using System;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public abstract class PackageChannelBase<TPackage> : ChannelBase, IChannel<TPackage>, IChannel where TPackage : class
    {
        public event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;

        public abstract Task SendAsync(TPackage package);
    }
}
