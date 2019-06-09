using System.Collections.Generic;
using System.Text;

namespace EasyProxy.HttpServer
{
    public class Constants
    {
        public static IDictionary<int, string> ReasonPhrase = new Dictionary<int, string>
        {
            { 200,"OK"},
            { 400,"Bad Request"},
            { 401,"Unauthorized"},
            { 403,"Forbidden"},
            { 404,"Not Found"},
            { 500,"Internal Server Error"},
            { 503,"Server Unavailable"}
        };

        public static Encoding DefaultEncoding = Encoding.UTF8;
    }
}
