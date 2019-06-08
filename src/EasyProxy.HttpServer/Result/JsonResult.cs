using Newtonsoft.Json;

namespace EasyProxy.HttpServer.Result
{
    public class JsonResult : IActionResult
    {
        public JsonResult(object data)
        {
            Data = data;
        }
        public object Data { get; set; }
        public HttpResponse ExecuteResult()
        {
            var result = new HttpResponse
            {
                ContentType = "application/json"
            };
            result.Body.Write(Constants.DefaultEncoding.GetBytes(JsonConvert.SerializeObject(Data)));
            result.Body.Seek(0, System.IO.SeekOrigin.Begin);
            return result;
        }
    }
}
