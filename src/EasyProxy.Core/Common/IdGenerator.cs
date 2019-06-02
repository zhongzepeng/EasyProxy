using System;
using System.Threading;

namespace EasyProxy.Core.Common
{
    public class IdGenerator : IIdGenerator
    {
        private long current = 0;

        public long Next()
        {
            Console.WriteLine($"currentId:{current},{Thread.CurrentThread.ManagedThreadId}");
            return Interlocked.Increment(ref current);
        }
    }
}
