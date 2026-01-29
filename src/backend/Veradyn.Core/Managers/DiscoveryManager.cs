using System.Collections.Generic;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Managers
{
    public class DiscoveryManager
    {
        private readonly ITenantService _tenantService;

        public DiscoveryManager(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        public Dictionary<string, object> GetOpenIdConfiguration(string baseUrl, string tenantId = "default")
        {
            var tenant = _tenantService.GetTenant(tenantId);
            if (tenant == null) return null;

            var issuer = tenant.Issuer ?? baseUrl;
            if (!issuer.EndsWith("/")) issuer += "/"; // Normalize if needed, but usually issuer is exact.

            // Construct OIDC Discovery Document
            return new Dictionary<string, object>
            {
                { "issuer", issuer },
                { "authorization_endpoint", $"{baseUrl}authorize" },
                { "token_endpoint", $"{baseUrl}token" },
                { "userinfo_endpoint", $"{baseUrl}userinfo" },
                { "jwks_uri", $"{baseUrl}.well-known/jwks.json" },
                { "scopes_supported", new[] { "openid", "profile", "email", "offline_access" } },
                { "response_types_supported", new[] { "code", "token", "id_token" } }, // Restricted by tenant toggles in real logic
                { "grant_types_supported", new[] { "authorization_code", "client_credentials", "refresh_token" } },
                { "subject_types_supported", new[] { "public" } },
                { "id_token_signing_alg_values_supported", new[] { "RS256" } },
                { "code_challenge_methods_supported", new[] { "S256" } } // PKCE
            };
        }
    }
}
