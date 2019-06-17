using System;
using System.Buffers;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public interface IChannel
    {
        event Func<IChannel, ReadOnlySequence<byte>, Task<SequencePosition>> DataReceived;

        Task StartAsync();

        ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        event Func<IChannel, Task> Closed;

        void Close();
    }

    public interface IChannel<TPackage> : IChannel where TPackage : class
    {
        event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;

        Task SendAsync(TPackage package);
    }
}
