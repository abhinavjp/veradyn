using System.Collections.Generic;
using System.Linq;
using Veradyn.Core.Interfaces.Data;
using Veradyn.Core.Interfaces.Services;
using Veradyn.Core.Models.Domain;

namespace Veradyn.Core.Services
{
    public class TenantService : ITenantService
    {
        private readonly IUnitOfWork _uow;

        public TenantService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public IEnumerable<Tenant> GetAllTenants()
        {
            return _uow.Tenants.GetAll();
        }

        public Tenant GetTenant(string tenantId)
        {
            return _uow.Tenants.GetById(tenantId);
        }

        public Tenant GetTenantByIssuer(string issuer)
        {
            return _uow.Tenants.Find(t => t.Issuer == issuer).FirstOrDefault();
        }
    }

    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _uow;

        public ClientService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public Client GetClient(string clientId)
        {
            return _uow.Clients.GetById(clientId);
        }

        public bool ValidateRedirectUri(Client client, string redirectUri)
        {
            if (client == null || client.RedirectUris == null) return false;
            // Exact match required by spec
            return client.RedirectUris.Contains(redirectUri);
        }
    }
}
