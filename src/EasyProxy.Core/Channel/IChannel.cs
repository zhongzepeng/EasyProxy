using EasyProxy.Core.Codec;
using System;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public interface IChannel<TPackage> where TPackage : class
    {
        event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;

        event EventHandler Closed;
        Task StartAsync();
        Task SendAsync(TPackage package);
    }
}
