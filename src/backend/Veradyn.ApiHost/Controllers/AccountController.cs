using System.Web;
using System.Web.Http;
using System.Web.Security;
using Veradyn.Core.Configurators;

namespace Veradyn.ApiHost.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginRequest model)
        {
            if (model == null) return BadRequest();

            var manager = VeradynConfigurator.GetAccountManager();
            var user = manager.Authenticate(model.Username, model.Password);

            if (user != null)
            {
                // Set Cookie
                FormsAuthentication.SetAuthCookie(user.Id, false); // Using ID as the principal name
                return Ok(new { success = true, username = user.Username });
            }

            return BadRequest("Invalid credentials");
        }
    }
}
