using System;
using System.Threading;

namespace EasyProxy.Core.Common
{
    public class IdGenerator : IIdGenerator
    {
        private long current = 0;

        public long Next()
        {
            return Interlocked.Increment(ref current);
        }
    }
}
