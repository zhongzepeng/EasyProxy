using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

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
