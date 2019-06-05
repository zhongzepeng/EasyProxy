using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Text;

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

        public static T BytesToObject<T>(this byte[] data) where T : class
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static byte[] ObjectToBytes(this object obj)
        {
            var str = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(str);
        }
    }
}
