using System.Threading.Tasks;

namespace EasyProxy.Core
{
    public interface IProxyHost
    {
        Task StartAsync();

        Task StopAsync();
    }
}
