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
    public class TcpPipeChannel<TPackage> : PipeChannel<TPackage> where TPackage : class
    {
        private Socket socket;
        private List<ArraySegment<byte>> segmentsForSend;
        public TcpPipeChannel(Socket socket, ILogger logger, IPackageEncoder<TPackage> encoder, IPackageDecoder<TPackage> decoder)
            : base(logger, encoder, decoder)
        {
            this.socket = socket;
        }

        protected override void OnClosed()
        {
            socket = null;
            base.OnClosed();
        }

        public override void Close()
        {
            var tsocket = socket;
            if (tsocket == null)
            {
                return;
            }

            if (Interlocked.CompareExchange(ref socket, null, tsocket) == tsocket)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    tsocket.Close();
                }
            }
        }

        protected override async Task ProcessReadsAsync()
        {
            var pipe = new Pipe();

            var writing = FillPipeAsync(pipe.Writer);

            var reading = ReadPipeAsync(pipe.Reader);

            await Task.WhenAll(reading, writing);
        }

        protected override async ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer)
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

        private async Task FillPipeAsync(PipeWriter writer)
        {
            while (true)
            {
                try
                {
                    var bufferSize = 1024;
                    var memory = writer.GetMemory(bufferSize);

                    var read = await ReceiveAsync(memory);
                    if (read == 0)
                    { break; }
                    writer.Advance(read);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Exception happened in ReceiveAsync");
                    break;
                }

                var result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
            writer.Complete();
            Output.Writer.Complete();
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
    }
}
