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
    }
}
