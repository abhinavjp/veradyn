using System;
using System.Security.Cryptography;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Interfaces.Services;

namespace Veradyn.Core.Services
{
    public class KeyMaterialService : IKeyMaterialService
    {
        // In a real app, these keys would be rotated and persisted. 
        // For this demo/InMemory, we generate one at startup or hardcode a static test key.
        // We'll use a static RSA key for consistency across requests in this runnable instance.

        private static readonly RSACryptoServiceProvider _rsa;
        private static readonly string _keyId;

        static KeyMaterialService()
        {
            _rsa = new RSACryptoServiceProvider(2048);
            _keyId = Guid.NewGuid().ToString("N");
        }

        private readonly IJsonSerializer _json;

        public KeyMaterialService(IJsonSerializer json)
        {
            _json = json;
        }

        public string GetSigningKeyId() => _keyId;

        public string GetJwkSetJson()
        {
            var parameters = _rsa.ExportParameters(false);

            // Minimal JWK format
            var jwk = new
            {
                kty = "RSA",
                use = "sig",
                kid = _keyId,
                alg = "RS256",
                n = Base64UrlEncode(parameters.Modulus),
                e = Base64UrlEncode(parameters.Exponent)
            };

            var jwks = new
            {
                keys = new[] { jwk }
            };

            return _json.Serialize(jwks);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            if (input == null) return null;
            var output = Convert.ToBase64String(input)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
            return output;
        }
    }
}
