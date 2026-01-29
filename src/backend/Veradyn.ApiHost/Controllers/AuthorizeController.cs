using System;
using System.Web;
using System.Web.Http;
using System.Linq;
using Veradyn.Core.Configurators;
using Veradyn.Core.Models.Domain;

namespace Veradyn.ApiHost.Controllers
{
    public class AuthorizeController : ApiController
    {
        [HttpGet]
        [Route("authorize")]
        public IHttpActionResult Authorize(
            string client_id = null,
            string redirect_uri = null,
            string response_type = null,
            string scope = null,
            string state = null,
            string code_challenge = null,
            string code_challenge_method = null,
            string nonce = null)
        {
            var manager = VeradynConfigurator.GetAuthManager();

            // Get Current User (Simulated for minimal standalone, or use GenericPrincipal)
            User currentUser = null;
            if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                // In real app, we'd load the full user from DB based on ID in claim.
                // Here we just construct a minimal user or modify Manager to take ID.
                // Manager takes User object. We should probably just pass the ID or fetch it.
                // For now, let's assume we can fetch it, or if it's simpler, just pass the Principal Name as ID (if that was how we logged in).
                var username = HttpContext.Current.User.Identity.Name;
                // Hack: Fetch user by username using manager logic? 
                // We need to fetch the user to pass to ProcessAuthorizeRequest. 
                // Or ProcessAuthorizeRequest should take a UserId string.
                // Let's create a minimal user object wrapper for now.
                currentUser = new User { Id = username, Username = username };
                // Warning: Manager might expect TenantId etc. Ideally Manager fetches it.
                // Let's Update AuthManager to take userId instead? 
                // Or just assume AccountManager loaded it properly.
                // Let's pass a dummy user with just ID, assuming Manager logic (AuthCode creation) only needs ID.
                // AuthManager: "SubjectId = currentUser.Id". YES. It only uses ID.
            }

            var result = manager.ProcessAuthorizeRequest(
                client_id, redirect_uri, response_type, scope, state,
                code_challenge, code_challenge_method, nonce, currentUser);

            if (result.IsSuccess)
            {
                return Redirect(result.RedirectUri);
            }

            if (result.IsLoginRequired)
            {
                // Redirect to Angular Login
                // Assume UI is hosted at same origin /
                var currentUrl = Request.RequestUri.ToString();
                var loginUrl = $"/login?returnUrl={Uri.EscapeDataString(currentUrl)}";
                return Redirect(loginUrl);
            }

            // Error
            if (result.RedirectUri != null)
            {
                return Redirect(result.RedirectUri);
            }

            return BadRequest($"{result.Error}: {result.ErrorDescription}");
        }
    }
}
