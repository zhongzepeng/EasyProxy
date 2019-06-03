using System;
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
}
