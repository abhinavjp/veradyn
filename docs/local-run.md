# Local Run Guide

## Prerequisites
- Windows OS
- Visual Studio 2019/2022 or Build Tools for .NET 4.7.2
- Node.js (Latest LTS)
- .NET Framework 4.7.2 SDK

## Backend Setup (Veradyn.ApiHost)
1. Open `Veradyn.sln` in Visual Studio.
2. Set `Veradyn.ApiHost` as the **Startup Project**.
3. Ensure the project is configured to run on IIS Express (Project Properties > Web > IIS Express).
4. Note the HTTPS URL (e.g., `https://localhost:44300`).
   - If changed, update `Veradyn.Core/InMemory/Seed/DemoDataSeeder.cs` Issuer.
5. Press **F5** to run.
6. Verification:
   - Navigate to `https://localhost:44300/.well-known/openid-configuration`
   - You should see the OIDC Discovery JSON.

## Frontend Setup (veradyn-ui)
1. Open a terminal in `src/veradyn-ui`.
2. Run `npm install`.
3. Run `ng serve`.
4. Open `http://localhost:4200`.

## End-to-End Test
1. Ensure the Backend is running.
2. Construct an Authorization URL in your browser:
   ```
   https://localhost:44300/authorize?client_id=demo-client&response_type=code&redirect_uri=http://localhost:4200/signin-callback&scope=openid%20profile&state=123&code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM&code_challenge_method=S256
   ```
   *(Note: The code_challenge above corresponds to verifier "secret")*
3. You should be redirected to `/login`.
4. Login with:
   - Username: `alice`
   - Password: `password`
5. You should be redirected back to `http://localhost:4200/signin-callback` with a `code`.
6. Exchange the code using Postman or CURL (see `api-examples.md`).
