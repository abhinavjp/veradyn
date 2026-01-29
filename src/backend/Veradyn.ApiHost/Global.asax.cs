using System.Web.Http;
using Veradyn.Core.Configurators;

namespace Veradyn.ApiHost
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 1. Boot Core
            VeradynConfigurator.Configure();

            // 2. Configure WebAPI
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
