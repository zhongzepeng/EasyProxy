using System;

namespace EasyProxy.Core
{
    public static class ServiceLocator
    {
        public static IServiceProvider Instance { get; private set; }

        public static void Init(IServiceProvider provider)
        {
            Instance = provider;
        }

        public static T GetService<T>() where T : class
        {
            if (Instance == null)
            {
                return default;
            }

            return Instance.GetService(typeof(T)) as T;
        }
    }

}
