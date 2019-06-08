using EasyProxy.Core;
using EasyProxy.HttpServer.Controller;
using EasyProxy.HttpServer.Result;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EasyProxy.HttpServer.Route
{
    public class EasyRoute : IRoute
    {
        private static Dictionary<string, (Type, MethodInfo)> httpGetRoutes = new Dictionary<string, (Type, MethodInfo)>();
        private static Dictionary<string, (Type, MethodInfo)> httpPostRoutes = new Dictionary<string, (Type, MethodInfo)>();

        static EasyRoute()
        {
            var controllers = Assembly
                 .GetEntryAssembly()
                 .GetTypes()
                 .Where(type => !type.IsAbstract && typeof(IController).IsAssignableFrom(type));
            foreach (var controller in controllers)
            {
                var prefix = controller.GetCustomAttribute<PrefixAttribute>()?.Prefix ?? string.Empty;
                var actions = controller.GetMethods()
                    .Where(m => m.ReturnType.IsAssignableFrom(typeof(IActionResult))
                                && m.IsPublic
                                && !m.IsGenericMethod
                                && (m.IsDefined(typeof(HttpGetAttribute), false)
                                || m.IsDefined(typeof(HttpPostAttribute), false))
                                && (m.GetParameters().Count() == 1
                                            && m.GetParameters()
                                                .Any(x => x.ParameterType.IsClass
                                                        && !x.ParameterType.IsAbstract
                                                        && !x.ParameterType.IsGenericType)
                                   || !m.GetParameters().Any())
                    );

                var getActions = actions.Where(x => x.IsDefined(typeof(HttpGetAttribute), false));
                var postActions = actions.Where(x => x.IsDefined(typeof(HttpPostAttribute), false));

                foreach (var action in getActions)
                {
                    var attribute = action.GetCustomAttribute<HttpGetAttribute>();
                    var path = $"{prefix}{attribute.Path ?? action.Name}".ToLower();
                    httpGetRoutes.Add(path, (controller, action));
                }

                foreach (var action in postActions)
                {
                    var attribute = action.GetCustomAttribute<HttpPostAttribute>();
                    var path = $"{prefix}{attribute.Path ?? action.Name}".ToLower();
                    httpPostRoutes.Add(path, (controller, action));
                }
            }
        }

        public (IController, MethodInfo, object) Route(HttpRequest request)
        {
            var path = request.Url;
            Type controllerType = null;
            MethodInfo methodInfo = null;
            switch (request.HttpMethod)
            {
                case HttpMethod.Get:
                    if (httpGetRoutes.ContainsKey(path))
                    {
                        (controllerType, methodInfo) = httpGetRoutes[path];
                    }
                    break;
                case HttpMethod.Post:
                    if (httpPostRoutes.ContainsKey(path))
                    {
                        (controllerType, methodInfo) = httpPostRoutes[path];
                    }
                    break;
                default://Unsupport httpmethod
                    return (null, null, null);
            }
            var controllerObj = ServiceLocator.Instance.GetService(controllerType) as IController;
            //var controllerObj = Activator.CreateInstance(controllerType) as IController;
            object parameter = null;
            var parameterType = methodInfo.GetParameters().SingleOrDefault()?.ParameterType;
            if (parameterType != null)
            {
                parameter = GetHttpGetParameter(parameterType, request);
            }

            return (controllerObj, methodInfo, parameter);
        }

        private object GetHttpGetParameter(Type type, HttpRequest request)
        {
            if (request.HttpMethod == HttpMethod.Get)
            {
                var query = request.Query;
                var obj = Activator.CreateInstance(type);
                var properties = type.GetProperties().Where(x => !x.PropertyType.IsClass);

                foreach (var property in properties)
                {
                    if (request.Query.ContainsKey(property.Name.ToLower()))
                    {
                        property.SetValue(obj, request.Query[property.Name.ToLower()]);
                    }
                }

                return obj;
            }
            else
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var bodyString = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject(bodyString, type, new JsonSerializerSettings
                    {
                    });
                }
            }
        }

    }
}
