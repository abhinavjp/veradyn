using System.Web.Http;
using Newtonsoft.Json.Serialization;

namespace Veradyn.ApiHost
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // JSON Formatting
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            // Attribute Routing
            config.MapHttpAttributeRoutes();

            // Default Route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
