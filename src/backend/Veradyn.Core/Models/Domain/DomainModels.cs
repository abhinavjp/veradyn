using System;
using System.Collections.Generic;

namespace Veradyn.Core.Models.Domain
{
    public class Tenant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Issuer { get; set; } // e.g. https://idp.com/t/tenant1
        public bool IsActive { get; set; } = true;

        // Flow Toggles
        public bool EnableAuthCode { get; set; } = true;
        public bool EnableImplicit { get; set; } = false;
        public bool RequirePkce { get; set; } = true;
    }

    public class Client
    {
        public string ClientId { get; set; }
        public string ClientSecretHash { get; set; } // Stored hashed
        public string Name { get; set; }
        public string TenantId { get; set; }
        public bool Enabled { get; set; } = true;

        public List<string> RedirectUris { get; set; } = new List<string>();
        public List<string> AllowedScopes { get; set; } = new List<string>();
        public List<string> AllowedGrantTypes { get; set; } = new List<string>();

        public int AccessTokenLifetimeSeconds { get; set; } = 3600;
        public bool RequirePkce { get; set; } = true;
        public bool RequireConsent { get; set; } = true;
    }

    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();
    }

    public class AuthorizationCode
    {
        public string Code { get; set; }
        public string ClientId { get; set; }
        public string SubjectId { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; } // Space separated
        public DateTime ExpirationUtc { get; set; }
        public string CodeChallenge { get; set; }
        public string CodeChallengeMethod { get; set; }
        public string TenantId { get; set; }
        public string Nonce { get; set; }
        public bool Used { get; set; }
    }

    public class Token
    {
        public string Id { get; set; } // jti
        public string Type { get; set; } // AccessToken, RefreshToken, IdToken
        public string Value { get; set; } // The actual JWT or handle
        public DateTime ExpirationUtc { get; set; }
        public string ClientId { get; set; }
        public string SubjectId { get; set; }
        public string TenantId { get; set; }
        public bool Revoked { get; set; }
    }

    public class ConsentGrant
    {
        public string Id { get; set; }
        public string SubjectId { get; set; }
        public string ClientId { get; set; }
        public string Scopes { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ExpirationUtc { get; set; }
    }
}
