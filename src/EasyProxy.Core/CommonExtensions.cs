using System;
using System.Buffers;

namespace EasyProxy.Core
{
    public static class CommonExtensions
    {
        public static int ToInt(this ReadOnlySequence<byte> sequence)
        {
            return BitConverter.ToInt32(sequence.ToArray());
        }

        public static int ToInt(this ReadOnlySpan<byte> span)
        {
            return BitConverter.ToInt32(span);
        }

        public static long ToLong(this ReadOnlySpan<byte> span)
        {
            return BitConverter.ToInt64(span);
        }
    }
}
