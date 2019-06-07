using System.Threading.Tasks;

namespace EasyProxy.HttpServer
{
    internal class DefaultHttpHandler : IHttpHandler
    {
        public async Task<HttpResponse> ProcessAsync(HttpRequest httpRequest)
        {
            return HttpResponseHelper.CreateDefaultErrorResponse();
        }
    }
}
