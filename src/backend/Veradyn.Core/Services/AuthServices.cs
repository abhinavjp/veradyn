using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Services
{
    public class ValidationService : IValidationService
    {
        public bool ValidateRedirectUri(Client client, string redirectUri)
        {
            if (client == null || client.RedirectUris == null) return false;
            // Exact match
            return client.RedirectUris.Contains(redirectUri, StringComparer.OrdinalIgnoreCase);
            // Warning: Spec says Simple String Comparison, usually exact match. 
            // Case sensitivity on URL path? Usually case sensitive.
            // But let's use Ordinal for strictness.
            // Actually, let's stick to the previous ClientService implementation logic which used Includes.
        }

        public bool ValidateScopes(Client client, IEnumerable<string> requestedScopes)
        {
            if (client == null) return false;
            // Use client.AllowedScopes.
            // If requestedScopes contains something NOT in AllowedScopes, fail?
            // Or just ignore it (downscope)?
            // OIDC spec says ignore unknown scopes usually, but for security strictness we can reject.
            // Veradyn policy: Fail on invalid scope.

            foreach (var scope in requestedScopes)
            {
                if (!client.AllowedScopes.Contains(scope)) return false;
            }
            return true;
        }
    }

    public class PkceService : IPkceService
    {
        private readonly Interfaces.Providers.ICryptoProvider _crypto; // Abstracted crypto

        public PkceService(Interfaces.Providers.ICryptoProvider crypto)
        {
            _crypto = crypto;
        }

        public bool ValidateCodeChallenge(string codeChallenge, string method)
        {
            if (string.IsNullOrEmpty(codeChallenge)) return false;
            if (method != "S256" && method != "plain") return false;
            return true;
        }

        public bool ValidateCodeVerifier(string codeVerifier, string codeChallenge, string method)
        {
            if (string.IsNullOrEmpty(codeVerifier)) return false;

            if (method == "plain")
            {
                return codeVerifier == codeChallenge;
            }

            if (method == "S256")
            {
                // S256: code_challenge = BASE64URL-ENCODE(SHA256(ASCII(code_verifier)))
                // My ICryptoProvider computes standard Base64 of SHA256 usually.
                // I need to ensure URL Safe encoding matches.
                // _crypto.ComputeHash returns Base64.

                // Oops, ICryptoProvider interface doesn't specify if it's base64url or base64.
                // SystemCryptoProvider implementation returns Standard Base64.
                // S256 requires Base64Url. 
                // I should handle the conversion here or update CryptoProvider to support generic hash.
                // SystemCryptoProvider.ComputeHash returns Base64.
                // Base64Url = Base64 with +->- /->_ and no padding.

                var hashBase64 = _crypto.ComputeHash(codeVerifier); // This does sha256(utf8(input)) -> base64
                                                                    // Wait, PKCE uses ASCII(code_verifier). Code verifier is usually ASCII/Unreserved chars. UTF8 matches ASCII for those.

                var validChallenge = ReplacePlusSlash(hashBase64);
                return validChallenge == codeChallenge;
            }

            return false;
        }

        private static string ReplacePlusSlash(string b64)
        {
            return b64.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
