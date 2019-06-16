using EasyProxy.Core.Codec;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public class ProxyChannel : PipeChannel
    {
        protected Socket socket;
        private List<ArraySegment<byte>> segmentsForSend;
        private readonly ChannelOptions options;
        private readonly CancellationToken cancellationToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        public ProxyChannel(Socket socket, ILogger logger, ChannelOptions options) : base(logger)
        {
            this.socket = socket;
            this.options = options;
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
        }

        protected bool IsClosed
        {
            get
            {
                return cancellationToken.IsCancellationRequested;
            }
        }

        protected override void OnClosed()
        {
            //socket = null;
            base.OnClosed();
        }

        public override void Close()
        {
            //var tsocket = socket;
            //if (tsocket == null)
            //{
            //    return;
            //}

            //if (Interlocked.CompareExchange(ref socket, null, tsocket) == tsocket)
            //{
            //    try
            //    {
            //        socket?.Shutdown(SocketShutdown.Both);
            //    }
            //    finally
            //    {
            //        tsocket?.Close();
            //    }
            //}
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                socket?.Close();
            }
            socket = null;
            cancellationTokenSource.Cancel();
        }

        protected override async Task ProcessReadAsync()
        {
            var pipe = new Pipe();
            var writting = FillPipeAsync(pipe.Writer);
            var reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writting);
        }

        private async Task FillPipeAsync(PipeWriter writer)
        {
            while (!IsClosed)
            {
                try
                {
                    var bufferSize = options.ReceiveBufferSize;
                    var maxPackageLength = options.MaxPackageLength;
                    if (maxPackageLength > 0)
                    {
                        bufferSize = Math.Min(bufferSize, maxPackageLength);
                    }

                    var memory = writer.GetMemory(bufferSize);

                    var read = await ReceiveAsync(memory);
                    if (read == 0)
                    {
                        continue;
                        //break;
                    }
                    writer.Advance(read);
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == 10054 || socketException.ErrorCode == 995)
                    {
                        logger.LogInformation("channel close");
                    }
                    else
                    {
                        logger.LogError(socketException, "Exception happened in ReceiveAsync");
                    }
                    break;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception happened in ReceiveAsync");
                    break;
                }

                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    logger.LogWarning($"completed:{result.IsCompleted}");
                    break;
                }
            }
            writer.Complete();
            output.Writer.Complete();
        }

        private async Task<int> ReceiveAsync(Memory<byte> memory)
        {
            return await socket.ReceiveAsync(GetArrayByMemory((ReadOnlyMemory<byte>)memory), SocketFlags.None);
        }

        private ArraySegment<T> GetArrayByMemory<T>(ReadOnlyMemory<T> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }


        protected async Task ReadPipeAsync(PipeReader reader)
        {
            while (!IsClosed)
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
                        if (buffer.Length <= 0)
                        {
                            break;
                        }

                        var curConsumed = await OnDataReceived(buffer);

                        if (consumed.Equals(curConsumed))//长度不够了，直接退出循环
                        {
                            break;
                        }
                        consumed = curConsumed;

                        buffer = buffer.Slice(consumed);
                    }
                    if (completed)
                        break;
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }
        }

        protected override async Task<int> SendAsync(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return await socket.SendAsync(GetArrayByMemory(buffer.First), SocketFlags.None);
            }

            if (segmentsForSend == null)
            {
                segmentsForSend = new List<ArraySegment<byte>>();
            }
            else
            {
                segmentsForSend.Clear();
            }

            foreach (var piece in buffer)
            {
                segmentsForSend.Add(GetArrayByMemory(piece));
            }

            return await socket.SendAsync(segmentsForSend, SocketFlags.None);
        }
    }

    public class ProxyChannel<TPackage> : ProxyChannel, IChannel<TPackage> where TPackage : class
    {
        private readonly IPackageEncoder<TPackage> encoder;
        private readonly IPackageDecoder<TPackage> decoder;
        public ProxyChannel(Socket socket, IPackageEncoder<TPackage> encoder, IPackageDecoder<TPackage> decoder, ILogger logger, ChannelOptions options)
            : base(socket, logger, options)
        {
            this.encoder = encoder;
            this.decoder = decoder;
            DataReceived += OnDataReceived;
        }

        public event Func<IChannel<TPackage>, TPackage, Task> PackageReceived;

        private async Task<SequencePosition> OnDataReceived(IChannel channel, ReadOnlySequence<byte> buffer)
        {
            var consumed = buffer.Start;

            if (buffer.Length < ProxyPackage.HEADER_SIZE)
            {
                return consumed;
            }
            var frameLength = buffer.Slice(consumed, ProxyPackage.HEADER_SIZE).ToInt();

            if (buffer.Length < ProxyPackage.HEADER_SIZE + frameLength)
            {
                return consumed;
            }
            var body = buffer.Slice(consumed, frameLength + ProxyPackage.HEADER_SIZE).ToArray();

            var package = decoder.Decode(body);

            consumed = buffer.GetPosition(frameLength + ProxyPackage.HEADER_SIZE);

            await OnPackageReceived(package);

            return consumed;
        }

        protected async Task OnPackageReceived(TPackage package)
        {
            await PackageReceived?.Invoke(this, package);
        }

        public async Task SendAsync(TPackage package)
        {
            var writer = output.Writer;
            await encoder.EncodeAsync(writer, package);
            await writer.FlushAsync();
        }
    }
}
