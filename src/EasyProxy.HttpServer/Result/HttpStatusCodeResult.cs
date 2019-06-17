using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Result
{
    public class HttpStatusCodeResult : IActionResult
    {
        public int StatusCode { get; set; } = 200;

        public async Task<HttpResponse> ExecuteResultAsync()
        {
            await Task.CompletedTask;
            return new HttpResponse()
            {
                StatusCode = StatusCode
            };
        }
    }
}
