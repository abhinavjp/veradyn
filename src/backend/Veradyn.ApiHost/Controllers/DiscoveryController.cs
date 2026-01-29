using System.Web.Http;
using Veradyn.Core.Configurators;

namespace Veradyn.ApiHost.Controllers
{
    [RoutePrefix(".well-known")]
    public class DiscoveryController : ApiController
    {
        [HttpGet]
        [Route("openid-configuration")]
        public IHttpActionResult GetConfiguration()
        {
            // 1. Resolve Dependencies via Configurator
            // In a full DI setup, this would be injected.
            var manager = VeradynConfigurator.GetDiscoveryManager();

            // 2. Determine Context (Base URL, Tenant)
            // For now, hardcode / infer from request
            var baseUrl = Request.RequestUri.GetLeftPart(System.UriPartial.Authority) + "/";
            // TODO: Extract tenant from path if multi-tenant

            // 3. Delegate to Manager
            var config = manager.GetOpenIdConfiguration(baseUrl);

            if (config == null) return NotFound();

            return Ok(config);
        }

        [HttpGet]
        [Route("jwks.json")]
        public IHttpActionResult GetJwks()
        {
            var manager = VeradynConfigurator.GetJwkManager();
            var jwks = manager.GetJwkSet();

            // Return as raw JSON content
            return ResponseMessage(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(jwks, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
