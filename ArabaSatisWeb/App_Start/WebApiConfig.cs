using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ArabaSatisWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // CORS yapılandırmasını ekleyin
            var cors = new EnableCorsAttribute("*", "*", "*");  // "*" tüm domainlerden gelen taleplere izin verir
            config.EnableCors(cors);  // CORS yapılandırmasını etkinleştir

            // Web API yapılandırması ve hizmetler
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
