using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EasyProxy.Core
{
    public class PipelineUtils
    {
        /// <summary>
        /// 读取socket 的内容,放入pipe中
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="pipeWriter"></param>
        /// <returns></returns>
        public static async Task FillPipeAsync(Socket socket, PipeWriter pipeWriter)
        {
            while (true)
            {
                var memory = pipeWriter.GetMemory(1024);

                try
                {
                    var read = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (read == 0)
                    {
                        break;
                    }
                    pipeWriter.Advance(read);
                }
                catch (Exception)
                {
                    break;
                }
                var result = await pipeWriter.FlushAsync();
                if (result.IsCompleted)
                {
                    break;
                }
            }
            pipeWriter.Complete();
        }

        /// <summary>
        /// 从pipe中取出数据发送
        /// </summary>
        /// <param name="pipeReader"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static async Task ReadPipeAsync(PipeReader pipeReader, int bufferLength, Action<byte[]> action)
        {
            while (true)
            {
                var result = await pipeReader.ReadAsync();

                if (result.IsCanceled)
                {
                    break;
                }
                var isCompleted = result.IsCompleted;

                var buffer = result.Buffer;

                var total = Math.Min(bufferLength, buffer.Length);

                var array = buffer.Slice(0, total).ToArray();//有多少发多少

                action(array);

                var examined = buffer.GetPosition(total);

                pipeReader.AdvanceTo(buffer.Start, examined);

                if (isCompleted)
                { break; }
            }

            pipeReader.Complete();
        }

        public static ArraySegment<byte> GetArray(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
    }
}
