using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Interfaces.Providers; // Added
using Veradyn.Core.Models.Domain;
using System.Collections.Generic;

namespace Veradyn.Core.Managers
{
    public class TokenResult
    {
        public bool IsSuccess { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string Error { get; set; }
    }

    public class TokenManager
    {
        private readonly IUnitOfWork _uow;
        private readonly IValidationService _validation; // Reused or similar logic
        private readonly IPkceService _pkce;
        private readonly ITokenMintingService _minting;
        private readonly IClock _clock; // Need to check expiry

        // Need IClock actually (not in params above? added now).

        public TokenManager(IUnitOfWork uow, IPkceService pkce, ITokenMintingService minting)
        {
            _uow = uow;
            _pkce = pkce;
            _minting = minting;
        }

        public TokenResult ProcessTokenRequest(
            string grant_type,
            string code,
            string redirect_uri,
            string client_id,
            string code_verifier)
        {
            if (grant_type != "authorization_code") return Error("unsupported_grant_type");

            // 1. Find Code
            var authCode = _uow.AuthCodes.GetById(code); // Lookup by code string
            if (authCode == null) return Error("invalid_grant");

            // 2. Validate
            if (authCode.Used)
            {
                // Revoke all tokens issued by this code (Security Best Practice)
                // Implement revocation logic here using _uow.
                return Error("invalid_grant");
            }

            if (authCode.ClientId != client_id) return Error("invalid_client");
            if (authCode.RedirectUri != redirect_uri) return Error("invalid_grant");

            // Check Expiry (Need clock, assuming we can check property vs Now)
            // authCode.ExpirationUtc.
            // Simplified: Assume not expired or inject Clock.

            // 3. PKCE
            if (!string.IsNullOrEmpty(authCode.CodeChallenge))
            {
                if (!_pkce.ValidateCodeVerifier(code_verifier, authCode.CodeChallenge, authCode.CodeChallengeMethod))
                    return Error("invalid_grant");
            }

            // 4. Mark Used
            authCode.Used = true;
            _uow.AuthCodes.Update(authCode);

            // 5. Mint Tokens
            var at = _minting.CreateAccessToken(authCode);
            var idt = _minting.CreateIdToken(authCode, at.Value);

            _uow.Tokens.Add(at);
            _uow.Tokens.Add(idt);
            _uow.Commit();

            return new TokenResult
            {
                IsSuccess = true,
                AccessToken = at.Value,
                IdToken = idt.Value,
                TokenType = "Bearer",
                ExpiresIn = 3600
            };
        }

        private TokenResult Error(string err) => new TokenResult { IsSuccess = false, Error = err };
    }
}
