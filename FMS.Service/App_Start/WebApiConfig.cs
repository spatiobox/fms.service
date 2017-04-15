using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;

namespace FMS.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


            // 版本： 在这替换控制器选择器
            //config.Services.Replace(typeof(IHttpControllerSelector), new NamespaceHttpControllerSelector(config));

            // 干掉XML序列化器
            var formatter = config.Formatters;
            formatter.Remove(config.Formatters.XmlFormatter);

            var json = formatter.JsonFormatter;
            // 解决json序列化时的循环引用问题
            json.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //对 JSON 的日期数据进行格式化
            var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            json.SerializerSettings.Converters.Add(dateTimeConverter);
            // 对 JSON 数据使用混合大小写。驼峰式,但是是javascript 首字母小写形式.
            //json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // 对 JSON 数据使用混合大小写。跟属性名同样的大小.输出
            //config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver();
            //config.Formatters.Add(new JsonPatchFormatter());

            json.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json-patch+json"));
        }
    }
}
