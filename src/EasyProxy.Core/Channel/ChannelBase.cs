using System;
using System.Buffers;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public abstract class ChannelBase<TPackage> : IChannel<TPackage>, IChannel where TPackage : class
    {
        public event EventHandler Closed;

        public event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;

        public abstract void Close();

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected async Task OnPackageReceived(TPackage package)
        {
            await PackageReceived?.Invoke(this, package);
        }

        public abstract Task SendAsync(TPackage package);

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        public abstract Task StartAsync();
    }

    public abstract class ChannelBase : IChannel
    {
        public event Func<IChannel, ReadOnlySequence<byte>, Task<SequencePosition>> DataReceived;

        public event EventHandler Closed;

        public abstract void Close();

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
        protected virtual async Task<SequencePosition> OnDataReceivedAsync(ReadOnlySequence<byte> buffer)
        {
            return await DataReceived?.Invoke(this, buffer);
        }
        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> data);

        public abstract Task StartAsync();
    }
}
