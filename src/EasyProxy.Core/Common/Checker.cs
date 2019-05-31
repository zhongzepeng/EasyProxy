using System;

namespace EasyProxy.Core.Common
{
    public class Checker
    {
        public static void NotNull<TCheckType>(TCheckType check, string message = "")
        {
            if (check != null)
            {
                return;
            }

            message = message ?? $"{typeof(TCheckType).Name} can not be null";

            throw new Exception(message);
        }
    }
}
