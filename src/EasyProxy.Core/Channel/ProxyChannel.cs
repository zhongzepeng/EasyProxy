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
        private readonly Pipe outputPipe;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        public ProxyChannel(Socket socket, IPackageEncoder<ProxyPackage> encoder, IPackageDecoder<ProxyPackage> decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder;
            this.socket = socket;
            pipe = new Pipe();
            outputPipe = new Pipe();
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
            //写入pipewriter
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
                        var package = ReadPackage(buffer, out int total);

                        if (package != null)
                        {
                            await OnPackageReceived(package);
                            examined = buffer.GetPosition(total);
                            buffer = buffer.Slice(total);
                            if (buffer.Length == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
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

        public ProxyPackage ReadPackage(ReadOnlySequence<byte> buffer, out int total)
        {
            if (ProxyPackage.HEADER_SIZE > buffer.Length)
            {
                total = 0;
                return null;
            }

            var headerBuffer = buffer.Slice(0, ProxyPackage.HEADER_SIZE);
            var packageLength = headerBuffer.ToInt();

            total = packageLength + ProxyPackage.HEADER_SIZE;

            if (total > buffer.Length)
            {
                total = 0;
                return null;
            }

            var bodyBuffer = buffer.Slice(ProxyPackage.HEADER_SIZE, packageLength);

            return decoder.Decode(bodyBuffer);
        }
    }
}
