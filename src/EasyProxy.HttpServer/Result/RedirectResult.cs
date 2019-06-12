namespace EasyProxy.HttpServer.Result
{
    public class RedirectResult : IActionResult
    {
        public RedirectResult(string url)
        {
            Url = url;
        }
        public string Url { get; set; }

        public HttpResponse ExecuteResult()
        {
            var res = new HttpResponse
            {
                StatusCode = 302
            };
            res.Headers.Add("Location", Url);
            return res;
        }
    }
}
