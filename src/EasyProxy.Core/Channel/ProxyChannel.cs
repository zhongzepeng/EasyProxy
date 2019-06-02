using EasyProxy.Core.Codec;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public class ProxyChannel : IChannel<ProxyPackage>
    {
        private readonly Socket socket;
        private readonly Pipe pipe;//用户接收数据
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        private readonly Pipe sendPipe;//用于发送数据


        private List<ArraySegment<byte>> segmentsForSend;
        public ProxyChannel(Socket socket, IPackageEncoder<ProxyPackage> encoder, IPackageDecoder<ProxyPackage> decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder;
            this.socket = socket;
            pipe = new Pipe();
            sendPipe = new Pipe();
        }

        public event Func<IChannel<ProxyPackage>, ProxyPackage, Task> PackageReceived;

        public event EventHandler Closed;

        public async Task SendAsync(ProxyPackage package)
        {
            var packageData = (Memory<byte>)encoder.Encode(package);
            Console.WriteLine($"发送数据包：{packageData.Length}");
            var writer = sendPipe.Writer;
            var memory = writer.GetMemory(packageData.Length);
            packageData.CopyTo(memory);
            writer.Advance(packageData.Length);
            await writer.FlushAsync();
        }

        public async Task StartAsync()
        {
            _ = PipelineUtils.FillPipeAsync(socket, pipe.Writer);
            _ = ReadPipeAsync(pipe.Reader);

            _ = ReadSendPipeAsync(sendPipe.Reader);

            await Task.CompletedTask;
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
            var frameLength = headerBuffer.ToInt();

            total = frameLength + ProxyPackage.HEADER_SIZE;

            if (total > buffer.Length)
            {
                total = 0;
                return null;
            }

            var bodyBuffer = buffer.Slice(ProxyPackage.HEADER_SIZE, frameLength).ToArray();

            return decoder.Decode(bodyBuffer);
        }

        /// <summary>
        /// 读取sendPipe的内容，发送出去
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private async Task ReadSendPipeAsync(PipeReader reader)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.IsCanceled)
                {
                    break;
                }
                var isCompleted = result.IsCompleted;

                var buffer = result.Buffer;
                var end = buffer.End;
                if (!buffer.IsEmpty)
                {
                    try
                    {
                        await SendAsync(buffer);
                    }
                    catch (Exception e)
                    {
                        reader.Complete();
                        return;
                    }
                }
                reader.AdvanceTo(end);

                //var total = Math.Min(SEND_BUFFER_SIZE, buffer.Length);
                //var array = buffer.Slice(0, total).ToArray();//有多少发多少
                //if (array.Length == 0)
                //{
                //    continue;
                //}
                ////Console.WriteLine($"真实发送数据：{array.Length}");
                //await socket.SendAsync(array, SocketFlags.None);

                //var examined = buffer.GetPosition(total);
                //reader.AdvanceTo(buffer.Start, examined);

                if (isCompleted)
                { break; }
            }

            reader.Complete();
        }

        protected async ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return await socket.SendAsync(PipelineUtils.GetArray(buffer.First), SocketFlags.None);
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
                segmentsForSend.Add(PipelineUtils.GetArray(piece));
            }

            return await socket.SendAsync(segmentsForSend, SocketFlags.None);
        }
    }
}
