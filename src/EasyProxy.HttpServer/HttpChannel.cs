using EasyProxy.Core.Channel;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public class HttpChannel : ProxyChannel
    {
        public event Func<HttpChannel, HttpRequest, Task> HttpRequested;

        protected async Task OnHttpRequestedAsync(HttpRequest httpRequest)
        {
            await HttpRequested?.Invoke(this, httpRequest);
        }

        private readonly Encoding defaultEncoding = Encoding.Default;
        public HttpChannel(Socket socket, ILogger logger, ChannelOptions options) : base(socket, logger, options)
        {
            DataReceived += OnDataReceived;
        }

        private async Task<SequencePosition> OnDataReceived(IChannel channel, ReadOnlySequence<byte> buffer)
        {
            SequencePosition consumed = buffer.Start;
            var startPos = consumed;
            var httpRequest = new HttpRequest();
            var statusLine = ReadLine(buffer, out consumed);
            if (statusLine == null)
            {
                return startPos;
            }
            buffer = buffer.Slice(consumed);

            httpRequest.ParseStatusLine(statusLine);

            var headerLines = ReadHeaderLines(buffer, out consumed);

            if (headerLines == null)
            {
                return startPos;
            }
            httpRequest.ParseHeader(headerLines);

            var hasContent = httpRequest.Headers.ContainsKey("Content-Length");
            if (hasContent)
            {
                buffer = buffer.Slice(consumed);

                var contentLength = int.Parse(httpRequest.Headers["Content-Length"]);

                if (buffer.Length < contentLength)
                {
                    return startPos;
                }

                await httpRequest.WriteBodyAsync(buffer.Slice(0, contentLength).ToArray());
                return buffer.GetPosition(contentLength);
            }
            _ = OnHttpRequestedAsync(httpRequest);
            return consumed;
        }

        private List<string> ReadHeaderLines(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
        {
            consumed = buffer.Start;
            var tempPos = consumed;
            var result = new List<string>();
            string curLine;
            while ((curLine = ReadLine(buffer, out consumed)) != null)
            {
                if (curLine == "")//读取到空行
                {
                    return result;
                }
                result.Add(curLine);
                buffer = buffer.Slice(consumed);
            }
            consumed = tempPos;
            return null;
        }

        private string ReadLine(ReadOnlySequence<byte> buffer, out SequencePosition consumed)
        {
            try
            {
                consumed = buffer.Start;
                var rindex = buffer.PositionOf((byte)'\r');
                var nindex = buffer.PositionOf((byte)'\n');
                if (rindex.HasValue && nindex.HasValue)
                {
                    var lineSequence = buffer.Slice(consumed, nindex.Value);
                    var linebytes = lineSequence.Slice(0, lineSequence.Length - 1).ToArray();
                    consumed = buffer.GetPosition(linebytes.Length + 2);
                    return defaultEncoding.GetString(linebytes);
                }
                return null;

            }
            catch (Exception ee)
            {

                throw ee;
            }

        }

    }
}
