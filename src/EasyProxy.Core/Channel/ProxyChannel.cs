using EasyProxy.Core.Codec;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public class ProxyChannel : IChannel<ProxyPackage>
    {
        private readonly Socket socket;
        private readonly Pipe pipe;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        public ProxyChannel(Socket socket, IPackageEncoder<ProxyPackage> encoder)
        {
            this.encoder = encoder;
            this.socket = socket;
            pipe = new Pipe();
        }

        public event Func<IChannel<ProxyPackage>, ProxyPackage, Task> PackageReceived;

        public event EventHandler Closed;

        public async Task SendAsync(ProxyPackage package)
        {
            var menory = encoder.Encode(package);
            await socket.SendAsync(menory, SocketFlags.None);
        }

        public async Task StartAsync()
        {
            _ = PipelineUtils.FillPipeAsync(socket, pipe.Writer);

            _ = ReadPipeAsync(pipe.Reader);
        }

        protected async Task OnPackageReceived(ProxyPackage package)
        {
            await PackageReceived?.Invoke(this, package);
        }

        private async Task ReadPipeAsync(PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();

                var buffer = result.Buffer;

                var consumed = buffer.Start;
                var examined = buffer.End;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }
                    var completed = result.IsCompleted;

                    while (true)
                    {
                        var package = ReadPackage(buffer, out consumed, out examined);

                        if (package != null)
                        {
                            await OnPackageReceived(package);
                        }

                        if (examined.Equals(buffer.End))
                        {
                            break;
                        }

                        buffer = buffer.Slice(examined);
                    }
                    if (completed)
                    {
                        break;
                    }

                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }
            reader.Complete();
        }

        public ProxyPackage ReadPackage(ReadOnlySequence<byte> buffer, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = buffer.Start;
            examined = buffer.End;

            if (ProxyPackage.HEADER_SIZE > buffer.Length)
            {
                return null;
            }

            var packageLength = buffer.Slice(0, ProxyPackage.HEADER_SIZE).ToInt(); //data length

            if (packageLength + ProxyPackage.HEADER_SIZE > buffer.Length)
            {
                return null;
            }


            return null;
        }
    }
}
