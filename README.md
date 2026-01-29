# Veradyn

Veradyn is a standalone **Identity Provider (IdP)** and **Authorization Server** built with **.NET Framework 4.7.2** and **Angular**. 
It implements the **OAuth 2.0** and **OpenID Connect (OIDC)** protocols, featuring a strict **DCM (Dependency Configurator Manager)** architecture and **In-Memory** data storage for easy standalone execution.

## 🚀 Features
- **OIDC Complaint**: Supports `/.well-known/openid-configuration`, `jwks_uri`.
- **Authorization Code Flow**: Secure flow with **PKCE** (Proof Key for Code Exchange) enforcement.
- **Standalone**: No external database required (In-Memory Unit of Work).
- **Secure by Default**: RS256 signing, S256 PKCE challenges, Validation Logic.
- **Angular UI**: Custom Login and Consent pages.

## 🛠️ Technology Stack
- **Backend**: .NET Framework 4.7.2 (Web API 2)
- **Frontend**: Angular 17+ (Standalone Components)
- **Testing**: NUnit
- **Storage**: In-Memory (Thread-safe)

## 📋 Prerequisites
- **Windows OS**
- **Visual Studio 2019/2022** (or Build Tools for .NET 4.7.2)
- **Node.js** (LTS Version) & NPM

## 🏃‍♂️ Getting Started

### 1. Backend (API)
1. Open `Veradyn.sln` in Visual Studio.
2. Set `Veradyn.ApiHost` as the **Startup Project**.
3. Run the application (F5) using **IIS Express**.
4. Confirm it is running by visiting:  
   `https://localhost:44300/.well-known/openid-configuration`

### 2. Frontend (UI)
1. Navigate to the UI directory:
   ```powershell
   cd src/veradyn-ui
   ```
2. Install dependencies:
   ```powershell
   npm install
   ```
3. Start the development server:
   ```powershell
   ng serve
   ```
4. Application will be available at: `http://localhost:4200`

## 🔑 Default Credentials
Use these credentials to sign in to the demo tenant:
- **Username**: `alice`
- **Password**: `password`

## 📚 Documentation
Detailed documentation is available in the `docs/` folder:
- [Architecture Overview](docs/architecture.md)
- [OIDC Flows & API](docs/oidc-flows.md)
- [Local Run Guide](docs/local-run.md)
- [API Examples](docs/api-examples.md)

## 🧪 Testing
Run the backend unit tests via Visual Studio Test Explorer or CLI:
```powershell
dotnet test tests\Veradyn.Tests.Core
```

## 📄 License
Private / Proprietary.
