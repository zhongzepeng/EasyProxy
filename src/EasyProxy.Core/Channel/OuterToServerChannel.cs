using System;
using System.Threading.Tasks;

namespace EasyProxy.Core.Channel
{
    public class OuterToServerChannel : ChannelBase
    {
        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            throw new NotImplementedException();
        }

        public override Task StartAsync()
        {
            throw new NotImplementedException();
        }
    }
}
