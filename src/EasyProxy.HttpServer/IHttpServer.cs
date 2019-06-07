using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    public interface IHttpServer
    {
        Task ListenAsync();
    }
}
