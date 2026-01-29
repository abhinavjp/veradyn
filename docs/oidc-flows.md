# Veradyn OIDC Flows

## Authorization Code Flow with PKCE
This is the default and recommended flow for all clients in Veradyn.

### 1. Authorization Request
**Endpoint**: `GET /authorize`
**Parameters**:
- `client_id`: Required. Must match registered client.
- `redirect_uri`: Required. Must match registered URI exactly.
- `response_type`: Must be `code`.
- `scope`: Space-separated list (e.g., `openid profile email`).
- `state`: Opaque value used to maintain state between request and callback.
- `code_challenge`: Base64UrlEncoded SHA256 hash of `code_verifier`.
- `code_challenge_method`: Must be `S256`.

**Example**:
```http
GET /authorize?client_id=demo-client&response_type=code&redirect_uri=http://localhost:4200/callback&scope=openid&state=xyz&code_challenge=...&code_challenge_method=S256
```

### 2. User Authentication & Consent
1. If the user is not authenticated, Veradyn redirects to `/login`.
2. User provides credentials.
3. Veradyn establishes a session (Cookie).
4. Veradyn checks for Consent (Scope Grants). If missing, redirects to `/consent`.
5. Upon approval, Veradyn generates an `authorization_code`.

### 3. Callback
Veradyn redirects the user agent back to `redirect_uri` with the code.
```http
HTTP/1.1 302 Found
Location: http://localhost:4200/callback?code=AUTH_CODE_123&state=xyz
```

### 4. Token Exchange
**Endpoint**: `POST /token`
**Parameters**:
- `grant_type`: Must be `authorization_code`.
- `code`: The code received in callback.
- `redirect_uri`: Must match the original request.
- `client_id`: Required.
- `code_verifier`: The plain string used to generate the challenge.

**Example**:
```http
POST /token
Content-Type: application/json

{
  "grant_type": "authorization_code",
  "code": "AUTH_CODE_123",
  "redirect_uri": "http://localhost:4200/callback",
  "client_id": "demo-client",
  "code_verifier": "SECRET_VERIFIER_STRING"
}
```

### 5. Response
```json
{
  "access_token": "eyJhbGci...",
  "id_token": "eyJhbGci...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

## Error Codes
| Error | Description |
|---|---|
| `invalid_request` | Missing parameter or malformed request. |
| `unauthorized_client` | Client not permitted to use this flow. |
| `access_denied` | User or Server denied the request. |
| `unsupported_response_type` | Only `code` is supported. |
| `invalid_scope` | Requested scope is invalid or unknown. |
| `server_error` | Internal server error. |
