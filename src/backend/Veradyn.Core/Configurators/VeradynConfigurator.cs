using Veradyn.Core.InMemory.Data;
using Veradyn.Core.InMemory.Seed;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Managers;
using Veradyn.Core.Services;

namespace Veradyn.Core.Configurators
{
    // Simple Static Composition Root (Manual DI)
    // In a real app, integrate this with Unity/Autofac/Ninject
    public static class VeradynConfigurator
    {
        // Global Singletons
        private static IJsonSerializer _json;
        private static ICryptoProvider _crypto;
        private static IClock _clock;
        private static ISecureRandom _random;
        private static IKeyMaterialService _keyService;

        public static void Configure()
        {
            // 1. Providers
            _random = new SystemSecureRandom();
            _clock = new SystemClock();
            _json = new NewtonsoftJsonSerializer();
            _crypto = new SystemCryptoProvider(_random);
            _keyService = new KeyMaterialService(_json);

            // 2. Seed Data
            DemoDataSeeder.Seed(_crypto);
        }

        // Factory method for per-request Managers
        // This is what ApiHost calls to get the work done.
        public static DiscoveryManager GetDiscoveryManager()
        {
            // Managers are transient/scoped usually.
            // But here they are lightweight.
            // Dependencies:
            var uow = new InMemoryUnitOfWork(); // New UoW per request
            var tenantService = new TenantService(uow);

            return new DiscoveryManager(tenantService);
        }

        public static JwkManager GetJwkManager()
        {
            // JwkManager doesn't need UoW, just KeyService (Singleton)
            return new JwkManager(_keyService);
        }

        public static AuthManager GetAuthManager()
        {
            var uow = new InMemoryUnitOfWork();
            var tenantService = new TenantService(uow);
            var clientService = new ClientService(uow);
            var validationService = new ValidationService();
            var pkceService = new PkceService(_crypto);

            return new AuthManager(uow, clientService, validationService, pkceService, _crypto, _clock);
        }

        public static AccountManager GetAccountManager()
        {
            var uow = new InMemoryUnitOfWork();
            return new AccountManager(uow, _crypto);
        }

        public static TokenManager GetTokenManager()
        {
            var uow = new InMemoryUnitOfWork();
            var pkce = new PkceService(_crypto);
            var minting = new TokenMintingService(_json, _keyService, _clock, _crypto);
            return new TokenManager(uow, pkce, minting);
        }
    }
}

