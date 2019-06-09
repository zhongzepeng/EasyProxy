namespace EasyProxy.HttpServer.Result
{
    public class HttpStatusCodeResult : IActionResult
    {
        public int StatusCode { get; set; } = 200;
        public HttpResponse ExecuteResult()
        {
            return new HttpResponse()
            {
                StatusCode = StatusCode
            };
        }
    }
}
