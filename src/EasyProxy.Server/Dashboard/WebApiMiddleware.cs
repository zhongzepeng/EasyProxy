//using EasyProxy.Server.Dashboard.Model;
//using Microsoft.AspNetCore.Http;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace EasyProxy.Server.Dashboard
//{
//    public class WebApiMiddleware
//    {
//        private readonly RequestDelegate next;
//        private readonly List<ApiDescription> apiDescriptions = new List<ApiDescription>();
//        private readonly string basePath;

//        public WebApiMiddleware(RequestDelegate next, string basePath)
//        {
//            this.next = next;
//            this.basePath = basePath;
//            var apis = typeof(WebApiMiddleware).GetMethods()
//                .ToList()
//                .Where(mi => mi.IsDefined(typeof(EasyRouteAttribute), true))
//                .Where(mi => mi.GetParameters().Length <= 1)
//                .Where(mi => !mi.IsAbstract && !mi.IsGenericMethod)
//                .Where(mi => mi.ReturnType.IsClass);

//            foreach (var api in apis)
//            {
//                var route = api.GetCustomAttributes(typeof(EasyRouteAttribute), true).First() as EasyRouteAttribute;
//                var parameter = api.GetParameters().FirstOrDefault();
//                var argumentCount = api.GetParameters().Length;
//                apiDescriptions.Add(new ApiDescription
//                {
//                    ArgumentCount = argumentCount,
//                    ArgumentType = argumentCount > 0 ? parameter.GetType() : default,
//                    HttpMethod = route.HttpMethod,
//                    MethodName = api.Name,
//                    Path = route.Path
//                });
//            }

//        }

//        public async Task Invoke(HttpContext httpContext)
//        {
//            var path = GetPath(httpContext.Request.Path);
//            var api = apiDescriptions.FirstOrDefault();

//            if (api == null)
//            {
//                await next(httpContext);
//                return;
//            }
//            if (api.ArgumentCount > 0)
//            {

//            }
//            GetType().GetMethod(api.MethodName).Invoke(this,)
//        }


//        private string GetPath(string fullPath)
//        {
//            return fullPath.Remove(0, basePath.Length);
//        }

//        [EasyRoute(Path = "/login")]
//        public BaseApiResult Login(LoginModel model)
//        {
//            return new BaseApiResult { };
//        }

//        [EasyRoute(Path = "/clients", HttpMethod = "GET")]
//        public BaseApiResult GetAllClient()
//        {
//            return null;
//        }
//    }
//}
