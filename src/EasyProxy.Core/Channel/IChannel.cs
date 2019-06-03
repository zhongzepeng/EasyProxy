using System;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public interface IChannel
    {
        Task StartAsync();

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        event EventHandler Closed;

        void Close();
    }

    public interface IChannel<TPackage> : IChannel where TPackage : class
    {
        event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;
        Task SendAsync(TPackage package);
    }
}
