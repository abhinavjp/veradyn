using System;
using System.Text;
using System.Collections.Generic;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Services
{
    public class TokenMintingService : ITokenMintingService
    {
        private readonly IJsonSerializer _json;
        private readonly IKeyMaterialService _keys;
        private readonly IClock _clock;
        private readonly ICryptoProvider _crypto;

        public TokenMintingService(IJsonSerializer json, IKeyMaterialService keys, IClock clock, ICryptoProvider crypto)
        {
            _json = json;
            _keys = keys;
            _clock = clock;
            _crypto = crypto;
        }

        public Token CreateAccessToken(AuthorizationCode code)
        {
            // Simple JWT Minting
            var type = "at+jwt"; // roughly
            var now = _clock.UtcNow;

            // Payload
            var payload = new Dictionary<string, object>
            {
                { "iss", "https://veradyn-idp" }, // Retrieve from Tenancy logic ideally
                { "sub", code.SubjectId },
                { "aud", "api" }, // Audience
                { "iat", ToUnixTime(now) },
                { "exp", ToUnixTime(now.AddSeconds(3600)) },
                { "client_id", code.ClientId },
                { "scope", code.Scope }
            };

            var jwt = Sign(payload);

            return new Token
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = "AccessToken",
                Value = jwt,
                ExpirationUtc = now.AddSeconds(3600),
                ClientId = code.ClientId,
                SubjectId = code.SubjectId,
                TenantId = code.TenantId
            };
        }

        public Token CreateIdToken(AuthorizationCode code, string accessToken)
        {
            var now = _clock.UtcNow;
            var payload = new Dictionary<string, object>
            {
                { "iss", "https://veradyn-idp" },
                { "sub", code.SubjectId },
                { "aud", code.ClientId },
                { "iat", ToUnixTime(now) },
                { "exp", ToUnixTime(now.AddMinutes(5)) },
                { "nonce", code.Nonce }
                // c_hash would go here if we calculated it
            };

            var jwt = Sign(payload);

            return new Token
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = "IdToken",
                Value = jwt,
                ExpirationUtc = now.AddMinutes(5),
                ClientId = code.ClientId,
                SubjectId = code.SubjectId,
                TenantId = code.TenantId
            };
        }

        private string Sign(Dictionary<string, object> payload)
        {
            // Header
            var header = new { alg = "RS256", typ = "JWT", kid = _keys.GetSigningKeyId() };
            var headerJson = _json.Serialize(header);
            var headerB64 = Base64Url(Encoding.UTF8.GetBytes(headerJson));

            // Payload
            var payloadJson = _json.Serialize(payload);
            var payloadB64 = Base64Url(Encoding.UTF8.GetBytes(payloadJson));

            var dataToSign = $"{headerB64}.{payloadB64}";

            // Signature (Abstracted via Crypto? But RS256 needs Private Key)
            // CryptoProvider interface had generic methods. 
            // We need RSA Signing. 
            // Since TokenMintingService is in Core.Services, and KeyMaterialService holds the RSA key (internal/static),
            // We should ask KeyMaterialService to sign? OR Update ICryptoProvider.
            // But KeyMaterialService has the private key (simulated).
            // Let's add SignData to IKeyMaterialService or assume TokenMintingService can access a signer.

            // Refactor: Add `Sign(string data)` to `IKeyMaterialService`.
            // For now, I will use a hack or just assume Unsigned for demo if Interface restricts.
            // BUT strict requirement "Standards aligned".
            // I'll assume we can add `Sign` to KeyMaterialService in a following tool call or cast it.
            // NOTE: I cannot change IKeyMaterialService easily right here without multi-editing.
            // I'll update IKeyMaterialService in the next step to support signing.

            // Placeholder
            return $"{headerB64}.{payloadB64}.[SIGNATURE_PLACEHOLDER]";
        }

        private long ToUnixTime(DateTime dt)
        {
            return (long)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private string Base64Url(byte[] input)
        {
            return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
