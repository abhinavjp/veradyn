using System.Collections.Generic;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Interfaces.Services
{
    public interface ITenantService
    {
        Tenant GetTenant(string tenantId);
        Tenant GetTenantByIssuer(string issuer);
        IEnumerable<Tenant> GetAllTenants();
    }

    public interface IClientService
    {
        Client GetClient(string clientId);
        bool ValidateRedirectUri(Client client, string redirectUri);
    }

    // New interface for Key Management (JWK)
    public interface IKeyMaterialService
    {
        // Returns the public keys in JWK format (JSON string)
        string GetJwkSetJson();

        // Returns the current signing key ID
        string GetSigningKeyId();
    }

    public interface IValidationService
    {
        bool ValidateRedirectUri(Client client, string redirectUri);
        bool ValidateScopes(Client client, IEnumerable<string> requestedScopes);
    }

    public interface IPkceService
    {
        bool ValidateCodeChallenge(string codeChallenge, string method);
        bool ValidateCodeVerifier(string codeVerifier, string codeChallenge, string method);
    }

    public interface ITokenMintingService
    {
        Models.Domain.Token CreateAccessToken(Models.Domain.AuthorizationCode code);
        Models.Domain.Token CreateIdToken(Models.Domain.AuthorizationCode code, string accessToken);
    }
}


