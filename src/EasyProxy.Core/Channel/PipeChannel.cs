using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public abstract class PipeChannel : ChannelBase, IChannel
    {
        protected readonly Pipe output;

        protected readonly ILogger logger;

        protected readonly object lockObj = new object();

        public PipeChannel(ILogger logger)
        {
            output = new Pipe();

            this.logger = logger;
        }

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            var writer = output.Writer;
            await writer.WriteAsync(data);
            await writer.FlushAsync();
        }
        protected abstract Task<int> SendAsync(ReadOnlySequence<byte> buffer);


        public override async Task StartAsync()
        {
            try
            {
                var readTask = ProcessReadAsync();
                var sendTask = ProcessSendAsync();

                await Task.WhenAll(readTask, sendTask);

                logger.LogInformation("call on closed");
                OnClosedAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Unhandled exception in PipeChannel.StartAsync,Channel:{this}");
            }
        }

        private async Task ProcessSendAsync()
        {
            var reader = output.Reader;
            while (true)
            {
                var result = await reader.ReadAsync();
                if (result.IsCompleted)
                {
                    break;
                }

                var completed = result.IsCompleted;

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
                        logger.LogError(e, "Exception happened in SendAsync.");
                        reader.Complete(e);
                        return;
                    }
                }
                reader.AdvanceTo(end);

                if (completed)
                { break; }
            }
            reader.Complete();
        }

        protected abstract Task ProcessReadAsync();
    }
}
