# API Examples

## Discovery
**Request**
```http
GET http://localhost:44300/.well-known/openid-configuration
```

**Response**
```json
{
    "issuer": "https://localhost:44300/",
    "authorization_endpoint": "https://localhost:44300/authorize",
    "token_endpoint": "https://localhost:44300/token",
    ...
}
```

## JWKS
**Request**
```http
GET http://localhost:44300/.well-known/jwks.json
```

## Token Exchange
**Request**
```http
POST http://localhost:44300/token
Content-Type: application/json

{
    "grant_type": "authorization_code",
    "code": "AUTH_CODE_FROM_CALLBACK",
    "client_id": "demo-client",
    "redirect_uri": "http://localhost:4200/signin-callback",
    "code_verifier": "secret" 
}
```
*Note: Ensure `code_verifier` matches the `code_challenge` sent in the authorize request. (S256 hash of "secret" is "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM")*

**Response**
```json
{
    "access_token": "...",
    "id_token": "...",
    "token_type": "Bearer",
    "expires_in": 3600
}
```
