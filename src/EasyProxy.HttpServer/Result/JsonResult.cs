﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace EasyProxy.HttpServer.Result
{
    public class JsonResult : IActionResult
    {
        public JsonResult(object data)
        {
            Data = data;
        }
        public object Data { get; set; }

        public async Task<HttpResponse> ExecuteResultAsync()
        {
            var result = new HttpResponse
            {
                ContentType = "application/json"
            };
            await result.Body.WriteAsync(Constants.DefaultEncoding.GetBytes(JsonConvert.SerializeObject(Data, serializerSettings)));
            result.Body.Seek(0, System.IO.SeekOrigin.Begin);
            return result;
        }

        private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.Indented
        };
    }
}
