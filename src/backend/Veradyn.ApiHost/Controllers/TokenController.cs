using System.Web.Http;
using Veradyn.Core.Configurators;

namespace Veradyn.ApiHost.Controllers
{
    public class TokenController : ApiController
    {
        public class TokenRequest
        {
            public string grant_type { get; set; }
            public string code { get; set; }
            public string redirect_uri { get; set; }
            public string client_id { get; set; }
            public string code_verifier { get; set; }
            // client_secret if basic auth not used
        }

        [HttpPost]
        [Route("token")]
        public IHttpActionResult Token([FromBody] TokenRequest req)
        {
            if (req == null) return BadRequest("invalid_request");

            var manager = VeradynConfigurator.GetTokenManager();
            var result = manager.ProcessTokenRequest(
                req.grant_type, req.code, req.redirect_uri, req.client_id, req.code_verifier);

            if (!result.IsSuccess)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, new { error = result.Error });
            }

            return Ok(new
            {
                access_token = result.AccessToken,
                id_token = result.IdToken,
                token_type = result.TokenType,
                expires_in = result.ExpiresIn
            });
        }
    }
}
