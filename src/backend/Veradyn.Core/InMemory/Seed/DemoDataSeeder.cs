using System;
using System.Collections.Generic;
using Veradyn.Core.InMemory.Data;
using Veradyn.Core.Interfaces.Providers;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.InMemory.Seed
{
    public static class DemoDataSeeder
    {
        public static void Seed(ICryptoProvider crypto)
        {
            InMemoryStore.Write(() =>
            {
                if (InMemoryStore.Tenants.Count > 0) return; // Already seeded

                // Tenants
                var t1 = new Tenant
                {
                    Id = "default",
                    Name = "Default Tenant",
                    Issuer = "https://localhost:44300",
                    EnableAuthCode = true,
                    RequirePkce = true
                };
                InMemoryStore.Tenants.Add(t1);

                // Clients
                var c1 = new Client
                {
                    ClientId = "demo-client",
                    Name = "Demo Angular App",
                    TenantId = "default",
                    Enabled = true,
                    RequirePkce = true,
                    RequireConsent = true,
                    ClientSecretHash = crypto.HashPassword("secret"), // simple hash for demo
                    RedirectUris = new List<string> { "http://localhost:4200/signin-callback" },
                    AllowedScopes = new List<string> { "openid", "profile", "email" },
                    AllowedGrantTypes = new List<string> { "authorization_code" }
                };
                InMemoryStore.Clients.Add(c1);

                // Users
                var u1 = new User
                {
                    Id = "u1",
                    Username = "alice",
                    PasswordHash = crypto.HashPassword("password"),
                    TenantId = "default",
                    IsActive = true,
                    Claims = new Dictionary<string, string>
                    {
                        { "name", "Alice Smith" },
                        { "email", "alice@veradyn.com" },
                        { "role", "admin" }
                    }
                };
                InMemoryStore.Users.Add(u1);
            });
        }
    }
}
