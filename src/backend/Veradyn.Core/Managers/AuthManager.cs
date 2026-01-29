using System;
using System.Linq;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;
namespace Veradyn.Core.Managers
{
    // Define Result DTOs here or in Models/Protocol
    public class AuthorizeResult

    {
        public bool IsSuccess { get; set; }
        public string RedirectUri { get; set; }
        public string Error { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsLoginRequired { get; set; }
    }

    public class AuthManager
    {
        private readonly IUnitOfWork _uow;
        private readonly IClientService _clientService;
        private readonly IValidationService _validationService;
        private readonly IPkceService _pkceService;
        private readonly ICryptoProvider _crypto;
        private readonly IClock _clock;

        public AuthManager(
            IUnitOfWork uow,
            IClientService clientService,
            IValidationService validationService,
            IPkceService pkceService,
            ICryptoProvider crypto,
            IClock clock)
        {
            _uow = uow;
            _clientService = clientService;
            _validationService = validationService;
            _pkceService = pkceService;
            _crypto = crypto;
            _clock = clock;
        }

        public AuthorizeResult ProcessAuthorizeRequest(
            string clientId,
            string redirectUri,
            string responseType,
            string scope,
            string state,
            string codeChallenge,
            string codeChallengeMethod,
            string nonce,
            User currentUser = null) // If user is logged in
        {
            // 1. Basic Validation
            var client = _clientService.GetClient(clientId);
            if (client == null) return Error("invalid_client", "Client not found.");

            if (!_validationService.ValidateRedirectUri(client, redirectUri))
                return Error("invalid_request", "Invalid redirect_uri.");

            // 2. Flow Validation
            if (responseType != "code")
                return Error("unsupported_response_type", "Only authorization_code flow is supported.", redirectUri, state);

            var scopes = (scope ?? "").Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (!_validationService.ValidateScopes(client, scopes))
                return Error("invalid_scope", "Invalid scope requested.", redirectUri, state);

            // 3. PKCE Check
            if (client.RequirePkce)
            {
                if (string.IsNullOrEmpty(codeChallenge))
                    return Error("invalid_request", "PKCE code_challenge is required.", redirectUri, state);

                if (!_pkceService.ValidateCodeChallenge(codeChallenge, codeChallengeMethod))
                    return Error("invalid_request", "Invalid code_challenge or method.", redirectUri, state);
            }

            // 4. User Authentication Check
            if (currentUser == null)
            {
                // Return result indicating login is needed
                return new AuthorizeResult { IsSuccess = false, IsLoginRequired = true };
            }

            // 5. Generate Code
            var code = _crypto.GenerateSecureToken(32); // 32 bytes -> base64
            var authCode = new AuthorizationCode
            {
                Code = code,
                ClientId = clientId,
                SubjectId = currentUser.Id,
                RedirectUri = redirectUri,
                Scope = scope,
                ExpirationUtc = _clock.UtcNow.AddSeconds(60), // 60s lifetime
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                TenantId = client.TenantId,
                Nonce = nonce,
                Used = false
            };

            _uow.AuthCodes.Add(authCode);
            _uow.Commit();

            // 6. Return Success Redirect
            var callback = $"{redirectUri}?code={code}&state={state}";
            return new AuthorizeResult { IsSuccess = true, RedirectUri = callback };
        }

        private AuthorizeResult Error(string error, string desc, string redirectUri = null, string state = null)
        {
            if (redirectUri == null)
            {
                // Can't redirect, return error for display
                return new AuthorizeResult { IsSuccess = false, Error = error, ErrorDescription = desc };
            }

            var sep = redirectUri.Contains("?") ? "&" : "?";
            var uri = $"{redirectUri}{sep}error={error}&error_description={Uri.EscapeDataString(desc)}";
            if (!string.IsNullOrEmpty(state)) uri += $"&state={state}";

            return new AuthorizeResult { IsSuccess = false, RedirectUri = uri };
        }
    }
}
