using System;
using System.Buffers;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public abstract class ChannelBase : IChannel
    {
        public event EventHandler Closed;

        public event Func<IChannel, ReadOnlySequence<byte>, Task<SequencePosition>> DataReceived;

        public abstract void Close();

        protected virtual void OnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected async Task<SequencePosition> OnDataReceived(ReadOnlySequence<byte> sequence)
        {
            return await DataReceived?.Invoke(this, sequence);
        }

        public abstract ValueTask SendAsync(ReadOnlyMemory<byte> buffer);

        public abstract Task StartAsync();
    }
}
