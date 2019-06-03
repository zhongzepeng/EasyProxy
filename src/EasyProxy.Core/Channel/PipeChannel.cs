using EasyProxy.Core.Codec;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    /// <summary>
    /// TODO：PipeChannel<TPackage> 继承PipeChannel
    /// </summary>
    /// <typeparam name="TPackage"></typeparam>
    public abstract class PipeChannel<TPackage> : ChannelBase<TPackage>, IChannel<TPackage>, IChannel where TPackage : class
    {
        protected IPackageEncoder<TPackage> PackageEncoder { get; }
        protected IPackageDecoder<TPackage> PackageDecoder { get; }
        protected Pipe Output { get; }

        protected ILogger Logger { get; }

        public PipeChannel(ILogger logger, IPackageEncoder<TPackage> encoder, IPackageDecoder<TPackage> packageDecoder)
        {
            PackageEncoder = encoder;
            Logger = logger;
            PackageDecoder = packageDecoder;
            Output = new Pipe();
        }

        public override async Task StartAsync()
        {
            try
            {
                var readTask = ProcessReadsAsync();
                var sendTask = ProcessSendsAsync();

                await Task.WhenAll(readTask, sendTask);

                OnClosed();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception in the method PipeChannel.StartAsync.");
            }
        }

        protected abstract Task ProcessReadsAsync();

        protected abstract ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer);

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            var writer = Output.Writer;
            await writer.WriteAsync(buffer);
            await writer.FlushAsync();
        }

        public override async Task SendAsync(TPackage package)
        {
            var writer = Output.Writer;
            await PackageEncoder.EncodeAsync(writer, package);
            await writer.FlushAsync();
        }

        protected async Task ProcessSendsAsync()
        {
            var reader = Output.Reader;
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
                        Logger.LogError(e, "Exception happened in SendAsync");
                        reader.Complete(e);
                        return;
                    }
                }
                reader.AdvanceTo(end);

                if (completed)
                {
                    break;
                }
            }
            reader.Complete();
        }

        protected async Task ReadPipeAsync(PipeReader reader)
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
                        var package = ReadBuffer(buffer, out consumed);

                        if (package == null)
                        {
                            break;
                        }

                        await OnPackageReceived(package);

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

        private TPackage ReadBuffer(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
        {
            consumed = buffer.Start;

            if (buffer.Length < ProxyPackage.HEADER_SIZE)
            {
                return null;
            }
            var frameLength = buffer.Slice(consumed, ProxyPackage.HEADER_SIZE).ToInt();

            if (buffer.Length < ProxyPackage.HEADER_SIZE + frameLength)
            {
                return null;
            }
            var body = buffer.Slice(consumed, frameLength + ProxyPackage.HEADER_SIZE).ToArray();

            consumed = buffer.GetPosition(frameLength + ProxyPackage.HEADER_SIZE);

            return PackageDecoder.Decode(body);
        }
    }


    public abstract class PipeChannel : ChannelBase
    {
        protected Pipe Output { get; }

        protected ILogger Logger { get; }

        public PipeChannel(ILogger logger)
        {
            Logger = logger;
        }


        public override async Task StartAsync()
        {
            try
            {
                var readTask = ProcessReadsAsync();
                var sendTask = ProcessSendsAsync();

                await Task.WhenAll(readTask, sendTask);

                OnClosed();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unhandled exception in the method PipeChannel.StartAsync.");
            }
        }

        protected abstract Task ProcessReadsAsync();

        protected abstract ValueTask<int> SendAsync(ReadOnlySequence<byte> buffer);

        public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer)
        {
            var writer = Output.Writer;
            await writer.WriteAsync(buffer);
            await writer.FlushAsync();
        }

        protected async Task ProcessSendsAsync()
        {
            var reader = Output.Reader;
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
                        Logger.LogError(e, "Exception happened in SendAsync");
                        reader.Complete(e);
                        return;
                    }
                }
                reader.AdvanceTo(end);

                if (completed)
                {
                    break;
                }
            }
            reader.Complete();
        }

        protected async Task ReadPipeAsync(PipeReader reader)
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
                        if (buffer.Length <= 0)
                        {
                            break;
                        }

                        consumed = await OnDataReceivedAsync(buffer);

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
    }
}
